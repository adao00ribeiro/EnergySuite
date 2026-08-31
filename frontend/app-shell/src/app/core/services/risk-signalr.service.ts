import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface RiskCalculatedEvent {
  contractId: string;
  counterpartyName: string;
  financialExposure: number;
  markToMarket: number;
  riskCategory: string;
  calculatedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class RiskSignalrService {
  private hubConnection: signalR.HubConnection | null = null;
  public riskCalculated$ = new Subject<RiskCalculatedEvent>();

  public startConnection = () => {
    const hubUrl = environment.apiUrl.replace(/\/api\/v1\/?$/, '') + '/hubs/risk';
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl(hubUrl)
                            .withAutomaticReconnect()
                            .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connection started');
        this.addRiskListener();
      })
      .catch(err => console.log('Error while starting SignalR connection: ' + err));
  }

  private addRiskListener = () => {
    if (this.hubConnection) {
      this.hubConnection.on('ReceiveRiskCalculation', (data: RiskCalculatedEvent) => {
        this.riskCalculated$.next(data);
      });
    }
  }
}
