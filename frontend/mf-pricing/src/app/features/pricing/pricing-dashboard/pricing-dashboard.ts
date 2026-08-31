import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RiskMetricsComponent, RiskMetric } from '../components/risk-metrics/risk-metrics';
import { ForwardCurveChartComponent } from '../components/forward-curve-chart/forward-curve-chart';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NewSimulationDialogComponent } from '../components/new-simulation-dialog/new-simulation-dialog.component';
import { ProspectService } from '../../prospect/services/prospect.service';

@Component({
  selector: 'app-pricing-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RiskMetricsComponent,
    ForwardCurveChartComponent,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './pricing-dashboard.html',
  styleUrl: './pricing-dashboard.css'
})
export class PricingDashboardComponent {
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private prospectService = inject(ProspectService);

  currentDate = new Date();

  riskMetrics: RiskMetric[] = [
    {
      title: 'Value at Risk (VaR)',
      value: 'R$ 2.4M',
      trend: 'up',
      trendValue: '+12.3%',
      description: 'VaR 95% da posição consolidada'
    },
    {
      title: 'Exposição Financeira',
      value: 'R$ 18.7M',
      trend: 'down',
      trendValue: '-5.1%',
      description: 'Exposição total por contraparte'
    },
    {
      title: 'Mark-to-Market',
      value: 'R$ 3.2M',
      trend: 'up',
      trendValue: '+8.7%',
      description: 'Resultado não realizado'
    },
    {
      title: 'Volatilidade Implícita',
      value: '24.6%',
      trend: 'neutral',
      trendValue: '0.2%',
      description: 'Média ponderada dos contratos'
    }
  ];

  onNewSimulation() {
    const dialogRef = this.dialog.open(NewSimulationDialogComponent, {
      width: '500px',
      panelClass: 'glass-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.prospectService.createStudy({
          name: result.scenarioName,
          description: `Simulação de precificação para ${result.portfolio} (confiança ${result.confidenceLevel}%)`,
          model: 'PricingSimulation',
          startDate: result.targetDate.toISOString(),
          horizonMonths: 24
        }).subscribe({
          next: () => {
            this.snackBar.open(`Simulação "${result.scenarioName}" criada com sucesso!`, 'Fechar', { duration: 4000 });
          },
          error: (err) => {
            console.error('Error creating simulation:', err);
            this.snackBar.open('Falha ao criar a simulação. Tente novamente.', 'Fechar', {
              duration: 5000,
              panelClass: ['warn-snackbar']
            });
          }
        });
      }
    });
  }
}