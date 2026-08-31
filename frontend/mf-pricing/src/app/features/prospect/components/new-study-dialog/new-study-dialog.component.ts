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
  selector: 'app-new-study-dialog',
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
  templateUrl: './new-study-dialog.component.html',
  styleUrl: './new-study-dialog.component.scss'
})
export class NewStudyDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<NewStudyDialogComponent>);

  studyForm = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    model: ['', Validators.required],
    startDate: [new Date(), Validators.required],
    horizonMonths: [12, [Validators.required, Validators.min(1)]]
  });

  models = ['NEWAVE', 'DECOMP', 'DESSEM'];

  onSubmit() {
    if (this.studyForm.valid) {
      this.dialogRef.close(this.studyForm.value);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
