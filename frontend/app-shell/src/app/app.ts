import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RiskSignalrService } from './core/services/risk-signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, MatSnackBarModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private riskSignalrService = inject(RiskSignalrService);
  private snackBar = inject(MatSnackBar);

  ngOnInit() {
    this.riskSignalrService.startConnection();
    
    this.riskSignalrService.riskCalculated$.subscribe(risk => {
      this.snackBar.open(
        `Risco calculado para ${risk.counterpartyName}: ${risk.riskCategory} (R$ ${risk.financialExposure.toLocaleString()})`, 
        'Fechar', 
        {
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: risk.riskCategory === 'HIGH' ? ['error-snackbar'] : ['success-snackbar']
        }
      );
    });
  }
}
