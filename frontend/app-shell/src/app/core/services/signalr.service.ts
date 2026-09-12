import { Injectable, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import * as signalR from '@microsoft/signalr';

export interface SimulationProgressData {
  simulationId: string;
  tenantId: string;
  percentage: number;
  status: string;
  message: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;

  // Angular Signals para estado reativo nativo no Angular 18
  public progressState = signal<SimulationProgressData | null>(null);
  public isConnected = signal<boolean>(false);
  public connectionError = signal<string | null>(null);

  public startConnection(tenantId?: string): void {
    const baseUrl = environment.apiUrl ? environment.apiUrl.replace(/\/api\/v1\/?$/, '') : 'http://localhost:5000';
    const hubUrl = `${baseUrl}/hubs/etrm`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.isConnected.set(true);
        this.connectionError.set(null);
        console.log('[SignalRService] Connected to /hubs/etrm');

        if (tenantId) {
          this.joinTenantGroup(tenantId);
        }

        this.registerListeners();
      })
      .catch(err => {
        this.isConnected.set(false);
        this.connectionError.set(err.toString());
        console.warn('[SignalRService] Error connecting to /hubs/etrm:', err);
      });
  }

  public joinTenantGroup(tenantId: string): void {
    if (this.hubConnection && this.isConnected()) {
      this.hubConnection.invoke('JoinTenantGroup', tenantId)
        .catch(err => console.error('[SignalR] Error joining group:', err));
    }
  }

  private registerListeners(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveSimulationProgress', (data: SimulationProgressData) => {
      console.log('[SignalR] Simulation Progress:', data);
      this.progressState.set(data);
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.isConnected.set(false);
    }
  }
}
