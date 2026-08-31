import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';

@Component({
  selector: 'app-new-opportunity-dialog',
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
    MatIconModule,
    MatDatepickerModule
  ],
  templateUrl: './new-opportunity-dialog.component.html',
  styleUrl: './new-opportunity-dialog.component.scss'
})
export class NewOpportunityDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NewOpportunityDialogComponent>);

  isSubmitting = false;
  counterparties = ['Engie Brasil', 'BTG Pactual Energy', 'Delta Energia', 'Matrix Energia', 'Eletrobras', 'Auren Energia'];

  form = this.fb.group({
    title: ['', Validators.required],
    counterparty: ['', Validators.required],
    type: ['Compra', Validators.required],
    submarket: ['SE_CO', Validators.required],
    volumeMwMed: [10, [Validators.required, Validators.min(0.1)]],
    price: [145.50, [Validators.required, Validators.min(0.1)]],
    startDate: [new Date(), Validators.required],
    endDate: [new Date(new Date().setFullYear(new Date().getFullYear() + 1)), Validators.required]
  });

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isSubmitting = true;
    setTimeout(() => {
      this.isSubmitting = false;
      this.dialogRef.close(this.form.value);
    }, 600);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
