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
        this.result = response;
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

    this.portfolioService.approveOperation(this.data.opportunityId, this.data.opportunityId, this.data.volumeMwm).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.snackBar.open(response.message, 'Fechar', {
          duration: 5000,
          panelClass: response.success ? ['success-snackbar'] : ['error-snackbar']
        });
        if (response.success) {
          this.dialogRef.close(true);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        const message = err?.error?.message || 'Falha na validação de crédito pelo backend.';
        this.snackBar.open(message, 'Fechar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  close() {
    this.dialogRef.close(false);
  }
}