import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  error(message: string, duration = 0) {
    this.snackBar.open(message, 'Fechar', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }

  warning(message: string, duration = 6000) {
    this.snackBar.open(message, 'Fechar', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['warn-snackbar']
    });
  }

  success(message: string, duration = 4000) {
    this.snackBar.open(message, 'Fechar', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  info(message: string, duration = 5000) {
    this.snackBar.open(message, 'Fechar', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }
}
