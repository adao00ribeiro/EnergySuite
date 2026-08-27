import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { NgxEchartsModule, NGX_ECHARTS_CONFIG } from 'ngx-echarts';

@Component({
  selector: 'app-prospect-detail',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatCardModule, MatListModule, MatIconModule, MatButtonModule, NgxEchartsModule],
  providers: [
    {
      provide: NGX_ECHARTS_CONFIG,
      useFactory: () => ({ echarts: () => import('echarts') })
    }
  ],
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
    { id: 1, mes: 'Janeiro/2027', status: 'Pending', versions: 1 },
    { id: 2, mes: 'Fevereiro/2027', status: 'Pending', versions: 1 },
    { id: 3, mes: 'Março/2027', status: 'Pending', versions: 1 },
    { id: 4, mes: 'Abril/2027', status: 'Pending', versions: 1 },
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
      // Simulate backend sending logs
      this.logs.update(logs => [...logs, `[WORKER] Iniciando processamento do Estudo ${this.studyId}...`]);
      this.decks.forEach(d => d.status = 'Running');
      
      setTimeout(() => {
        this.logs.update(logs => [...logs, `[WORKER] Preparando Deck Mês 1 (01/2027)...`]);
        this.logs.update(logs => [...logs, `[WORKER] Simulação do Deck 1 concluída com sucesso.`]);
        this.decks[0].status = 'Completed';

        setTimeout(() => {
          this.logs.update(logs => [...logs, `[WORKER] Preparando Deck Mês 2 (02/2027)...`]);
          this.logs.update(logs => [...logs, `[ERROR] Inviabilidade encontrada no Deck Mês 2! (Déficit extremo de energia).`]);
          this.decks[1].status = 'Infeasible';

          setTimeout(() => {
            this.logs.update(logs => [...logs, `[WARNING] Sistema iniciando Auto-Relaxamento (injetando energia fictícia)...`]);
            this.logs.update(logs => [...logs, `[SYSTEM] Nova Versão 2 criada. Re-executando Deck Mês 2...`]);
            
            // Simula o auto-relaxamento
            this.decks[1].versions = 2;
            this.decks[1].status = 'Running';

            setTimeout(() => {
              this.logs.update(logs => [...logs, `[WORKER] Simulação do Deck 2 concluída com sucesso.`]);
              this.decks[1].status = 'Completed';
              this.decks[2].status = 'Completed';
              this.decks[3].status = 'Completed';
              this.logs.update(logs => [...logs, `[SYSTEM] Execução finalizada! Status do Estudo: COMPLETED.`]);
              
              this.loadResults(); // Load chart data after completion
            }, 2000);

          }, 1500);

        }, 1500);

      }, 1500);
      
    }, 1000);
  }

  results: any[] = [];
  chartOptions: any = null;

  loadResults() {
    // Simulando retorno da API `GET /api/v1/prospect/studies/{id}/results`
    this.results = [
      { month: '01/2027', pldSE: 89.4, pldS: 75.1, pldNE: 92.0, pldN: 60.5 },
      { month: '02/2027', pldSE: 110.2, pldS: 82.0, pldNE: 95.5, pldN: 65.0 },
      { month: '03/2027', pldSE: 150.0, pldS: 90.5, pldNE: 105.0, pldN: 68.2 },
      { month: '04/2027', pldSE: 120.5, pldS: 85.0, pldNE: 98.0, pldN: 62.1 }
    ];

    this.chartOptions = {
      title: { text: 'Projeção de PLD por Submercado (R$/MWh)' },
      tooltip: { trigger: 'axis' },
      legend: { data: ['SE/CO', 'S', 'NE', 'N'], bottom: 0 },
      xAxis: { type: 'category', data: this.results.map(r => r.month) },
      yAxis: { type: 'value' },
      series: [
        { name: 'SE/CO', type: 'line', data: this.results.map(r => r.pldSE) },
        { name: 'S', type: 'line', data: this.results.map(r => r.pldS) },
        { name: 'NE', type: 'line', data: this.results.map(r => r.pldNE) },
        { name: 'N', type: 'line', data: this.results.map(r => r.pldN) }
      ]
    };
  }

  exportToCsv() {
    const header = 'Mes,PLD SE/CO,PLD S,PLD NE,PLD N\n';
    const rows = this.results.map(r => `${r.month},${r.pldSE},${r.pldS},${r.pldNE},${r.pldN}`).join('\n');
    const csvContent = 'data:text/csv;charset=utf-8,' + header + rows;
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `Resultados_Estudo_${this.studyId}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
