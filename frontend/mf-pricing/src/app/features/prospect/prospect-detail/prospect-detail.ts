import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { NgxEchartsModule, NGX_ECHARTS_CONFIG } from 'ngx-echarts';
import { EChartsOption } from 'echarts';
import { ProspectService, StudyResult } from '../services/prospect.service';
import { environment } from '../../../../environments/environment';

interface NivelInicial {
  submercado: string;
  nivel: number;
}

interface PremissasEstudo {
  cenarioGsf: string;
  crescimentoCarga: number;
  niveisIniciais: NivelInicial[];
}

interface DeckItem {
  id: string;
  mes: string;
  status: string;
  versions: number;
}

@Component({
  selector: 'app-prospect-detail',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatCardModule, MatListModule, MatIconModule, MatButtonModule, MatSnackBarModule, NgxEchartsModule],
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
  private prospectService = inject(ProspectService);
  private snackBar = inject(MatSnackBar);

  studyId: string | null = null;
  logs = signal<string[]>([]);
  isExecuting = signal<boolean>(false);
  premissas = signal<PremissasEstudo | null>(null);
  decks = signal<DeckItem[]>([]);
  isHubConnected = signal<boolean>(false);
  private hubConnection: signalR.HubConnection | null = null;

  ngOnInit() {
    this.studyId = this.route.snapshot.paramMap.get('id');
    this.startSignalRConnection();
    this.loadResults();
  }

  ngOnDestroy() {
    if (this.hubConnection && this.studyId) {
      this.hubConnection.invoke('UnsubscribeFromStudy', this.studyId).catch(() => undefined);
      this.hubConnection.stop();
    }
  }

  startSignalRConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.prospectHubUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('LogReceived', (message: string) => {
      this.logs.update(logs => [...logs, message]);
    });

    this.hubConnection
      .start()
      .then(() => {
        this.isHubConnected.set(true);
        if (this.studyId) {
          this.hubConnection?.invoke('SubscribeToStudy', this.studyId);
        }
      })
      .catch(err => console.error('Erro ao conectar SignalR:', err));
  }

  executeStudy() {
    if (!this.studyId || this.isExecuting()) {
      return;
    }

    this.isExecuting.set(true);
    this.logs.update(logs => [...logs, `[SYSTEM] Enviando requisição POST para executar estudo ${this.studyId}...`]);

    this.prospectService.executeStudy(this.studyId).subscribe({
      next: () => {
        this.logs.update(logs => [...logs, `[SYSTEM] Execução enfileirada (202). Acompanhe os logs do trabalhador no console abaixo.`]);
      },
      error: (err) => {
        this.isExecuting.set(false);
        this.logs.update(logs => [...logs, `[ERROR] Não foi possível enfileirar a execução do estudo ${this.studyId}.`]);
        this.snackBar.open('Falha ao enfileirar a execução do estudo.', 'Fechar', {
          duration: 5000,
          panelClass: ['warn-snackbar']
        });
        console.error('Erro ao executar estudo', err);
      }
    });
  }

  results: StudyResult[] = [];
  chartOptions: EChartsOption | null = null;

  loadResults() {
    if (!this.studyId) {
      return;
    }

    this.prospectService.getStudyResults(this.studyId).subscribe({
      next: (response) => {
        this.results = response.results ?? [];

        if (this.results.length === 0) {
          this.chartOptions = null;
          return;
        }

        this.buildChart(this.results);
      },
      error: (err) => {
        this.chartOptions = null;
        console.error('Erro ao carregar resultados do estudo', err);
      }
    });
  }

  private buildChart(results: StudyResult[]) {
    this.chartOptions = {
      title: { text: 'Projeção de PLD por Submercado (R$/MWh)' },
      tooltip: { trigger: 'axis' },
      legend: { data: ['SE/CO', 'S', 'NE', 'N'], bottom: 0 },
      xAxis: { type: 'category', data: results.map(r => r.month) },
      yAxis: { type: 'value' },
      series: [
        { name: 'SE/CO', type: 'line', data: results.map(r => r.pldSE) },
        { name: 'S', type: 'line', data: results.map(r => r.pldS) },
        { name: 'NE', type: 'line', data: results.map(r => r.pldNE) },
        { name: 'N', type: 'line', data: results.map(r => r.pldN) }
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
