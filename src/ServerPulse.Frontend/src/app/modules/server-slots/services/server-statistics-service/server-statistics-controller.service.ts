import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environment/environment';
import { CustomErrorHandler } from '../../../shared';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root'
})
export class ServerStatisticsControllerService implements ServerStatisticsService {
  private static hubConnection: signalR.HubConnection | null = null;
  private static connectionPromise: Promise<void>;

  constructor(private readonly errorHandler: CustomErrorHandler) {
    if (!ServerStatisticsControllerService.hubConnection) {
      ServerStatisticsControllerService.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(environment.statisticsHub)
        .build();
    }
  }

  private get connectionPromise(): Promise<void> {
    return ServerStatisticsControllerService.connectionPromise;
  }
  private set connectionPromise(value: Promise<void>) {
    ServerStatisticsControllerService.connectionPromise = value;
  }
  private get hubConnection(): signalR.HubConnection {
    if (!ServerStatisticsControllerService.hubConnection) {
      throw new Error('Hub connection is not initialized.');
    }
    return ServerStatisticsControllerService.hubConnection;
  }

  startConnection(): Observable<void> {
    return new Observable<void>((observer) => {
      const hubState = this.hubConnection.state;
      if (hubState === signalR.HubConnectionState.Disconnected) {
        this.connectionPromise = this.hubConnection.start();
        this.connectionPromise
          .then(() => {
            observer.next();
            observer.complete();
          })
          .catch((error) => {
            this.errorHandler.handleHubError(error);
            observer.error(error);
          });
      } else if (hubState === signalR.HubConnectionState.Connecting) {
        this.connectionPromise
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
  receiveStatistics(): Observable<{ key: string, data: string }> {
    return new Observable<{ key: string, data: string }>((observer) => {
      this.hubConnection.on('ReceiveStatistics', (key: string, data: string) => {
        observer.next({ key, data });
      });
    });
  }
  startListenPulse(key: string): void {
    this.hubConnection.invoke('StartListenPulse', key).catch((err) => {
      this.errorHandler.handleHubError(err);
    });
  }
}