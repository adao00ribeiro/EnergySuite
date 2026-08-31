import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RiskSignalrService } from './core/services/risk-signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, MatSnackBarModule],
  templateUrl: './app.html'
})
export class App implements OnInit {
  private riskSignalrService = inject(RiskSignalrService);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  ngOnInit() {
    this.riskSignalrService.startConnection();

    this.riskSignalrService.riskCalculated$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(risk => {
      this.snackBar.open(
        `Risco calculado para ${risk.counterpartyName}: MtM R$ ${risk.markToMarket.toLocaleString(undefined, {minimumFractionDigits: 2, maximumFractionDigits: 2})} [${risk.riskCategory}]`,
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
