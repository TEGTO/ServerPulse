import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AuthenticationService } from '../../../authentication';
import { CustomErrorHandler } from '../../../shared';
import { RealTimeStatisticsCollector } from './realtime-statistics-collector';

@Injectable({
  providedIn: 'root',
})
export class StatisticsCollector implements OnDestroy, RealTimeStatisticsCollector {
  private hubConnections: Map<string, { connection: signalR.HubConnection, promise: Promise<void> }> = new Map();
  private authToken: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private readonly errorHandler: CustomErrorHandler,
    private readonly authService: AuthenticationService
  ) {
    this.authService.getAuthData()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        if (this.authToken !== data.accessToken && data.accessToken) {
          this.authToken = data.accessToken;
          this.deleteOldConnections();
        }
      });
  }

  startConnection(hubUrl: string): Observable<void> {
    return new Observable<void>((observer) => {
      let connectionEntry = this.hubConnections.get(hubUrl);
      if (!connectionEntry) {
        const hubConnection = this.createNewHubConnection(hubUrl);
        connectionEntry = { connection: hubConnection, promise: Promise.resolve() };
        this.hubConnections.set(hubUrl, connectionEntry);
      }

      const { connection } = connectionEntry;
      const hubState = connection.state;

      if (hubState === signalR.HubConnectionState.Disconnected && this.authToken) {
        this.setConnectionPromise(hubUrl,
          connection.start()
            .catch((error) => {
              this.errorHandler.handleHubError(error);
              observer.error(error);
            }));
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
    this.destroy$.next();
    this.destroy$.complete();
  }

  private deleteOldConnections() {
    this.hubConnections.forEach((entry, hubUrl) => {
      const hubState = entry.connection.state;
      if (hubState === signalR.HubConnectionState.Disconnected) {
        this.hubConnections.delete(hubUrl);
      }
    });
  }
  private createNewHubConnection(hubUrl: string): signalR.HubConnection {

    return new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}?access_token=${this.authToken || ''}`)
      .build();
  }

  private getHubConnection(hubUrl: string): signalR.HubConnection {
    const connectionEntry = this.hubConnections.get(hubUrl);
    if (!connectionEntry) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return connectionEntry.connection;
  }

  private getConnectionPromise(hubUrl: string): Promise<void> {
    const connectionEntry = this.hubConnections.get(hubUrl);
    if (!connectionEntry) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
    return connectionEntry.promise;
  }

  private setConnectionPromise(hubUrl: string, value: Promise<void>): void {
    const connectionEntry = this.hubConnections.get(hubUrl);
    if (connectionEntry) {
      connectionEntry.promise = value;
    } else {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }
  }
}