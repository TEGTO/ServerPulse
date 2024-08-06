import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../../../authentication';
import { CustomErrorHandler } from '../../../shared';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root',
})
export class ServerStatisticsControllerService implements OnDestroy, ServerStatisticsService {
  private static hubConnections: Map<string, { connection: signalR.HubConnection, promise: Promise<void> }> = new Map();
  private authToken: string | null = null;
  private refreshSubscription: any;

  constructor(
    private readonly errorHandler: CustomErrorHandler,
    private readonly authService: AuthenticationService
  ) {
    this.refreshSubscription = this.authService.getAuthData().subscribe(data => {
      this.authToken = data.accessToken;
      if (this.authToken) {
        this.refreshTokenAndReconnect();
      }
    });
  }

  startConnection(hubUrl: string): Observable<void> {
    return new Observable<void>((observer) => {
      let connectionEntry = ServerStatisticsControllerService.hubConnections.get(hubUrl);
      if (!connectionEntry) {
        const hubConnection = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl, {
            accessTokenFactory: () => this.authToken || ''
          })
          .build();

        connectionEntry = {
          connection: hubConnection,
          promise: Promise.resolve(),
        };
        ServerStatisticsControllerService.hubConnections.set(hubUrl, connectionEntry);
      }

      const { connection, promise } = connectionEntry;
      const hubState = connection.state;

      if (hubState === signalR.HubConnectionState.Disconnected && this.authToken) {
        this.setConnectionPromise(hubUrl, connection.start());
        this.getConnectionPromise(hubUrl)
          .then(() => {
            observer.next();
            observer.complete();
          })
          .catch((error) => {
            this.errorHandler.handleHubError(error);
            observer.error(error);
          });
      } else if (hubState === signalR.HubConnectionState.Connecting) {
        promise
          .then(() => {
            observer.next();
            observer.complete();
          })
          .catch((error) => {
            this.errorHandler.handleHubError(error);
            observer.error(error);
          });
      } else {
        observer.next();
        observer.complete();
      }
    });
  }

  receiveStatistics(hubUrl: string): Observable<{ key: string, data: string }> {
    return new Observable<{ key: string, data: string }>((observer) => {
      this.getHubConnection(hubUrl).on('ReceiveStatistics', (key: string, data: string) => {
        observer.next({ key, data });
      });
    });
  }

  startListen(hubUrl: string, key: string): void {
    this.getHubConnection(hubUrl).invoke('StartListen', key).catch((err) => {
      this.errorHandler.handleHubError(err);
    });
  }

  ngOnDestroy() {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
    }
  }

  private refreshTokenAndReconnect() {
    if (this.authToken) {
      ServerStatisticsControllerService.hubConnections.forEach((entry, hubUrl) => {
        if (entry.connection.state === signalR.HubConnectionState.Connected) {
          entry.connection.stop().then(() => {
            entry.connection.start();
          }).catch(err => this.errorHandler.handleHubError(err));
        }
      });
    }
  }

  private getHubConnection(hubUrl: string): signalR.HubConnection {
    if (!ServerStatisticsControllerService.hubConnections.has(hubUrl)) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return ServerStatisticsControllerService.hubConnections.get(hubUrl)!.connection;
  }

  private getConnectionPromise(hubUrl: string): Promise<void> {
    if (!ServerStatisticsControllerService.hubConnections.has(hubUrl)) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return ServerStatisticsControllerService.hubConnections.get(hubUrl)!.promise;
  }

  private setConnectionPromise(hubUrl: string, value: Promise<void>): void {
    if (ServerStatisticsControllerService.hubConnections.has(hubUrl)) {
      ServerStatisticsControllerService.hubConnections.get(hubUrl)!.promise = value;
    } else {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
  }
}