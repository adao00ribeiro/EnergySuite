import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RiskMetricsComponent } from '../components/risk-metrics/risk-metrics';
import { ForwardCurveChartComponent } from '../components/forward-curve-chart/forward-curve-chart';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { NewSimulationDialogComponent } from '../components/new-simulation-dialog/new-simulation-dialog.component';

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
  
  currentDate = new Date();

  onNewSimulation() {
    const dialogRef = this.dialog.open(NewSimulationDialogComponent, {
      width: '500px',
      panelClass: 'glass-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(`Simulação "${result.scenarioName}" enfileirada com sucesso!`, 'Fechar', { duration: 4000 });
        // Lógica de integração com o backend via Service (CQRS) entraria aqui
      }
    });
  }
}
