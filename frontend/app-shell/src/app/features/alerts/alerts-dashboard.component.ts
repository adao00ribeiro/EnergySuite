import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import * as signalR from '@microsoft/signalr';

export interface AlertMessage {
  id: string;
  category: 'System' | 'Risk' | 'Operational';
  severity: 'Info' | 'Warning' | 'Critical';
  title: string;
  message: string;
  timestamp: Date;
  read: boolean;
}

@Component({
  selector: 'app-alerts-dashboard',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatCardModule, MatIconModule, MatButtonModule, MatChipsModule, MatListModule],
  templateUrl: './alerts-dashboard.component.html',
  styleUrls: ['./alerts-dashboard.component.scss']
})
export class AlertsDashboardComponent implements OnInit, OnDestroy {
  private hubConnection: signalR.HubConnection | undefined;
  
  alerts: AlertMessage[] = [
    { id: '1', category: 'System', severity: 'Info', title: 'Atualização do Sistema', message: 'A release 2.4 foi implantada com sucesso.', timestamp: new Date(), read: false },
    { id: '2', category: 'Risk', severity: 'Critical', title: 'Limite de VaR Excedido', message: 'O portfólio SE excedeu o limite de VaR 95%.', timestamp: new Date(), read: false },
    { id: '3', category: 'Operational', severity: 'Warning', title: 'Falha no MLFlow', message: 'Execução do modelo de previsão falhou.', timestamp: new Date(), read: true }
  ];

  ngOnInit() {
    this.connectSignalR();
  }

  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private connectSignalR() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/hubs/alerts') // Assuming reverse proxy handles this path
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveAlert', (alert: AlertMessage) => {
      this.alerts = [alert, ...this.alerts];
    });

    this.hubConnection.start()
      .then(() => console.log('SignalR Alerts Hub connected.'))
      .catch(err => console.error('Error while starting connection: ' + err));
  }

  getAlertsByCategory(category: 'System' | 'Risk' | 'Operational') {
    return this.alerts.filter(a => a.category === category);
  }

  markAllAsRead() {
    this.alerts = this.alerts.map(a => ({ ...a, read: true }));
  }

  clearHistory() {
    this.alerts = [];
  }
}
