import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../../../authentication';
import { CustomErrorHandler } from '../../../shared';
import { RealTimeStatisticsCollector } from './realtime-statistics-collector';

@Injectable({
  providedIn: 'root',
})
export class StatisticsCollector implements OnDestroy, RealTimeStatisticsCollector {
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
      let connectionEntry = StatisticsCollector.hubConnections.get(hubUrl);
      if (!connectionEntry) {
        const hubConnection = this.createNewHubConnection(hubUrl);
        connectionEntry = { connection: hubConnection, promise: Promise.resolve() };
        StatisticsCollector.hubConnections.set(hubUrl, connectionEntry);
      }

      const { connection, promise } = connectionEntry;
      const hubState = connection.state;

      if (hubState === signalR.HubConnectionState.Disconnected && this.authToken) {
        this.setConnectionPromise(hubUrl, connection.start());
      } else if (hubState === signalR.HubConnectionState.Connecting) {
        // Do nothing, just wait for connection to complete
      } else {
        observer.next();
        observer.complete();
        return;
      }

      this.getConnectionPromise(hubUrl)
        .then(() => {
          observer.next();
          observer.complete();
        })
        .catch((error) => {
          this.errorHandler.handleHubError(error);
          observer.error(error);
        });
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
      StatisticsCollector.hubConnections.forEach((entry, hubUrl) => {
        const newConnection = this.createNewHubConnection(hubUrl);
        if (entry.connection.state === signalR.HubConnectionState.Connected) {
          entry.connection.stop().then(() => {
            this.setConnectionPromise(hubUrl, newConnection.start());
          }).catch(err => this.errorHandler.handleHubError(err));
        }
        else (entry.connection.state === signalR.HubConnectionState.Disconnected)
        {
          this.setConnectionPromise(hubUrl, newConnection.start());
        }
        this.getConnectionPromise(hubUrl).catch((error) => { this.errorHandler.handleHubError(error); });
      });
    }
  }

  private createNewHubConnection(hubUrl: string): signalR.HubConnection {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}?access_token=${this.authToken || ''}`)
      .build();
  }

  private getHubConnection(hubUrl: string): signalR.HubConnection {
    const connectionEntry = StatisticsCollector.hubConnections.get(hubUrl);
    if (!connectionEntry) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return connectionEntry.connection;
  }

  private getConnectionPromise(hubUrl: string): Promise<void> {
    const connectionEntry = StatisticsCollector.hubConnections.get(hubUrl);
    if (!connectionEntry) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return connectionEntry.promise;
  }

  private setConnectionPromise(hubUrl: string, value: Promise<void>): void {
    const connectionEntry = StatisticsCollector.hubConnections.get(hubUrl);
    if (connectionEntry) {
      connectionEntry.promise = value;
    } else {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
  }
}