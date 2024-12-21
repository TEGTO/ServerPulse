/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { BaseStatisticsResponse } from '../..';
import { ErrorHandler } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class SignalStatisticsService implements OnDestroy {
  private readonly hubConnections = new Map<string, signalR.HubConnection>();
  private readonly receiveStatisticsHubObservables = new Map<string, Observable<any>>();

  constructor(private readonly errorHandler: ErrorHandler) {
  }

  ngOnDestroy(): void {
    this.hubConnections.forEach((connection) => {
      connection.stop();
    });

    this.hubConnections.clear();
    this.receiveStatisticsHubObservables.clear();
  }

  startConnection(hubUrl: string, accessToken: string): Observable<void> {
    let connection = this.hubConnections.get(hubUrl);

    if (!connection) {
      const hubConnection = this.createNewHubConnection(hubUrl, accessToken);
      connection = hubConnection;
      this.hubConnections.set(hubUrl, connection);
    }

    return new Observable<void>((observer) => {
      if (connection.state === signalR.HubConnectionState.Connected) {
        observer.next();
        observer.complete();
        return;
      }

      const connectionState$ = new Observable<void>((stateObserver) => {
        const interval = setInterval(() => {
          if (connection.state === signalR.HubConnectionState.Connected) {
            clearInterval(interval);
            stateObserver.next();
            stateObserver.complete();
          }
        }, 100);

        return () => clearInterval(interval);
      });

      if (connection.state === signalR.HubConnectionState.Connecting) {
        connectionState$.subscribe(observer);
        return;
      }

      connection
        .start()
        .then(() => {
          observer.next();
          observer.complete();
        })
        .catch((err) => {
          this.errorHandler.handleHubError(err);
          observer.error(err);
        });
    });
  }

  stopConnection(hubUrl: string): void {
    const connection = this.hubConnections.get(hubUrl);

    if (connection) {
      connection.stop();
      this.hubConnections.delete(hubUrl);
      this.receiveStatisticsHubObservables.delete(hubUrl);
    }
  }

  receiveStatistics<T extends BaseStatisticsResponse>(hubUrl: string): Observable<{ key: string, response: T }> {
    if (this.receiveStatisticsHubObservables.has(hubUrl)) {
      return this.receiveStatisticsHubObservables.get(hubUrl) as Observable<{ key: string, response: T }>;
    }

    const observable = new Observable<{ key: string, response: T }>((observer) => {
      const connection = this.hubConnections.get(hubUrl);

      if (!connection) {
        throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
      }

      const handler = (key: string, response: T) => {
        console.log(`Received statistics for key: ${key}`, response);
        observer.next({ key, response });
      };

      connection.on('ReceiveStatistics', handler);

      return () => {
        connection.off('ReceiveStatistics', handler);
      };
    });

    this.receiveStatisticsHubObservables.set(hubUrl, observable);

    return observable;
  }

  startListen(hubUrl: string, key: string): void {
    const connection = this.hubConnections.get(hubUrl);

    if (!connection) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }

    connection.invoke('StartListen', key).catch((err) => {
      this.errorHandler.handleHubError(err);
    });
  }

  stopListen(hubUrl: string, key: string): void {
    const connection = this.hubConnections.get(hubUrl);

    if (!connection) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }

    connection.invoke('StopListen', key).catch((err) => {
      this.errorHandler.handleHubError(err);
    });
  }

  private createNewHubConnection(hubUrl: string, accessToken: string): signalR.HubConnection {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}?access_token=${accessToken ?? ''}`)
      .build();
  }
}
