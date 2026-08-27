import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';

@Component({
  selector: 'app-new-simulation-dialog',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule
  ],
  templateUrl: './new-simulation-dialog.component.html',
  styleUrls: ['./new-simulation-dialog.component.scss']
})
export class NewSimulationDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NewSimulationDialogComponent>);

  simulationForm = this.fb.group({
    scenarioName: ['', Validators.required],
    portfolio: ['', Validators.required],
    confidenceLevel: [95, Validators.required],
    targetDate: [new Date(), Validators.required]
  });

  portfolios = ['Portfólio Global', 'Submercado SE/CO', 'Submercado SUL', 'Submercado NE'];
  confidenceLevels = [
    { value: 90, label: '90%' },
    { value: 95, label: '95%' },
    { value: 99, label: '99%' }
  ];

  onSubmit() {
    if (this.simulationForm.valid) {
      this.dialogRef.close(this.simulationForm.value);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
