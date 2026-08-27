import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { CceeComparisonListComponent } from '../ccee-comparison-list/ccee-comparison-list';
import { CceeIntegrationService } from '../services/ccee-integration.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-ccee-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    CceeComparisonListComponent,
    MatSnackBarModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    ReactiveFormsModule
  ],
  templateUrl: './ccee-dashboard.html',
  styleUrls: ['./ccee-dashboard.scss']
})
export class CceeDashboardComponent {
  cceeService = inject(CceeIntegrationService);
  snackBar = inject(MatSnackBar);

  isUploading = signal(false);

  exportForm = new FormGroup({
    start: new FormControl<Date>(new Date(new Date().getFullYear(), new Date().getMonth(), 1)),
    end: new FormControl<Date>(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0))
  });

  exportCceal() {
    const start = this.exportForm.value.start;
    const end = this.exportForm.value.end;

    if (!start || !end) {
      this.snackBar.open('Selecione o período de exportação.', 'Fechar', { duration: 3000 });
      return;
    }

    const startStr = start.toISOString().split('T')[0];
    const endStr = end.toISOString().split('T')[0];

    this.cceeService.exportCceal(startStr, endStr).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `cceal_export_${startStr}_${endStr}.xml`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.isUploading.set(true);
      this.cceeService.uploadCliqCcee(file).subscribe({
        next: (res) => {
          this.isUploading.set(false);
          this.snackBar.open(res.message || 'Arquivo processado com sucesso.', 'Fechar', { duration: 5000 });
          this.cceeService.loadComparisons(); // Refresh the list
        },
        error: (err) => {
          this.isUploading.set(false);
          this.snackBar.open('Erro ao processar arquivo CCEE.', 'Fechar', { duration: 5000, panelClass: 'error-snackbar' });
        }
      });
    }
  }
}
