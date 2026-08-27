import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute } from '@angular/router';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-prospect-detail',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatCardModule, MatListModule, MatIconModule, MatButtonModule],
  templateUrl: './prospect-detail.html',
  styleUrls: ['./prospect-detail.css']
})
export class ProspectDetailComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);

  studyId: string | null = null;
  logs = signal<string[]>([]);
  private hubConnection: signalR.HubConnection | null = null;
  
  // Mock data for UI
  premissas = {
    cenarioGsf: 'Pessimista',
    crescimentoCarga: 2.5,
    niveisIniciais: [
      { submercado: 'SE/CO', nivel: 45.2 },
      { submercado: 'S', nivel: 80.1 },
      { submercado: 'NE', nivel: 60.5 },
      { submercado: 'N', nivel: 90.0 }
    ]
  };

  decks = [
    { mes: 'Janeiro/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Fevereiro/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Março/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Abril/2027', status: 'Pending', icon: 'hourglass_empty' },
  ];

  ngOnInit() {
    this.studyId = this.route.snapshot.paramMap.get('id');
    this.startSignalRConnection();
  }

  ngOnDestroy() {
    if (this.hubConnection && this.studyId) {
      this.hubConnection.invoke('UnsubscribeFromStudy', this.studyId);
      this.hubConnection.stop();
    }
  }

  startSignalRConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/hubs/prospect') // Backend API endpoint
      .build();

    this.hubConnection.on('LogReceived', (message: string) => {
      this.logs.update(logs => [...logs, message]);
    });

    this.hubConnection
      .start()
      .then(() => {
        if (this.studyId) {
          this.hubConnection?.invoke('SubscribeToStudy', this.studyId);
        }
      })
      .catch(err => console.error('Erro ao conectar SignalR:', err));
  }

  executeStudy() {
    // Fake HTTP request for UI demo
    this.logs.update(logs => [...logs, `[SYSTEM] Enviando requisição POST para executar estudo ${this.studyId}...`]);
    
    setTimeout(() => {
      // Simulate backend sending logs since backend is not actually responding here without full integrated network setup
      this.logs.update(logs => [...logs, `[WORKER] Iniciando processamento do Estudo ${this.studyId}...`]);
      this.decks.forEach(d => d.status = 'Running');
      
      setTimeout(() => {
        this.logs.update(logs => [...logs, `[WORKER] Preparando Deck Mês 1 (01/2027)...`]);
        this.logs.update(logs => [...logs, `[WORKER] Simulação do Deck 1 concluída com sucesso.`]);
        this.decks[0].status = 'Completed';
      }, 2000);
      
    }, 1000);
  }
}
