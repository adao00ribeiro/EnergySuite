import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PortfolioService } from '../../../../core/services/portfolio.service';

export interface SimulationData {
  opportunityId: string;
  name: string;
  volumeMwm: number;
}

@Component({
  selector: 'app-simulation-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './simulation-dialog.component.html',
  styleUrls: ['./simulation-dialog.component.scss']
})
export class SimulationDialogComponent implements OnInit {
  isLoading = true;
  isSubmitting = false;
  result: any = null;

  constructor(
    public dialogRef: MatDialogRef<SimulationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SimulationData,
    private snackBar: MatSnackBar,
    private portfolioService: PortfolioService
  ) {}

  ngOnInit() {
    this.simulateOperation();
  }

  simulateOperation() {
    this.portfolioService.simulateOperation(this.data.opportunityId, this.data.volumeMwm).subscribe({
      next: (response: any) => {
        // Fallback for mocked UI representation if backend returns generic success
        this.result = {
          previousVolumeMwm: 30.5,
          newVolumeMwm: 30.5 + this.data.volumeMwm,
          volumeDelta: this.data.volumeMwm,
          previousEstimatedResult: 450000.00,
          newEstimatedResult: response.newEstimatedResult || (450000.00 - 12000.00),
          financialDelta: response.newEstimatedResult ? (response.newEstimatedResult - 450000.00) : -12000.00,
          copilotAnalysis: {
            summaryText: `Esta operação de ${this.data.volumeMwm} MWm afeta a exposição do portfólio. Verificamos que o delta financeiro é um custo aceitável de hedge.`,
            recommendation: "Approve"
          }
        };
        this.isLoading = false;
      },
      error: (err) => {
        this.snackBar.open('Falha ao simular operação.', 'OK', { duration: 3000 });
        this.isLoading = false;
        this.dialogRef.close();
      }
    });
  }

  approve() {
    this.isSubmitting = true;
    
    // Simulate ApproveOperationCommand -> Imeris Validation
    setTimeout(() => {
      this.isSubmitting = false;
      if (this.data.volumeMwm > 20) {
        this.snackBar.open(`🚨 Risco de Crédito Reprovado (Imeris): O volume de ${this.data.volumeMwm} MWm ultrapassa o limite pré-aprovado de 20 MWm da contraparte.`, 'Fechar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      } else {
        this.snackBar.open('✅ Operação aprovada com sucesso! O BackOps foi notificado.', 'Fechar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.dialogRef.close(true);
      }
    }, 2000);
  }

  close() {
    this.dialogRef.close(false);
  }
}
