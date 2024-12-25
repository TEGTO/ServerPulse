/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { BaseStatisticsResponse } from '../..';
import { ErrorHandler } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class SignalStatisticsService implements OnDestroy {
  private readonly hubConnections = new Map<string, signalR.HubConnection>();
  private readonly receiveStatisticsHubObservables = new Map<string, Subject<any>>();

  constructor(private readonly errorHandler: ErrorHandler) {
  }

  ngOnDestroy(): void {
    this.receiveStatisticsHubObservables.forEach((observable) => {
      observable.complete();
    });

    this.receiveStatisticsHubObservables.clear();

    this.hubConnections.forEach((connection) => {
      connection.stop();
    });

    this.hubConnections.clear();
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
    if (this.receiveStatisticsHubObservables.has(hubUrl)) {
      this.receiveStatisticsHubObservables.get(hubUrl)!.complete();
      this.receiveStatisticsHubObservables.delete(hubUrl);
    }

    if (this.hubConnections.has(hubUrl)) {
      this.hubConnections.get(hubUrl)!.stop();
      this.hubConnections.delete(hubUrl);
    }
  }

  receiveStatistics<T extends BaseStatisticsResponse>(hubUrl: string): Subject<{ key: string, response: T }> {
    if (this.receiveStatisticsHubObservables.has(hubUrl)) {
      return this.receiveStatisticsHubObservables.get(hubUrl) as Subject<{ key: string, response: T }>;
    }

    const connection = this.hubConnections.get(hubUrl);
    if (!connection) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }

    const subject = new Subject<{ key: string, response: T }>();

    const handler = (key: string, response: T) => {
      subject.next({ key, response });
    };

    connection.on('ReceiveStatistics', handler);

    subject.subscribe({
      complete: () => {
        connection.off('ReceiveStatistics', handler);
      },
      error: (error) => this.errorHandler.handleHubError(error)
    });

    this.receiveStatisticsHubObservables.set(hubUrl, subject);

    return subject;
  }

  startListen(hubUrl: string, key: string, getInitial = true): void {
    const connection = this.hubConnections.get(hubUrl);

    if (!connection) {
      throw new Error(`Hub connection for ${hubUrl} is not initialized.`);
    }

    connection.invoke('StartListen', key, getInitial).catch((err) => {
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
