import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface EditRolesDialogData {
  userId: string;
  username: string;
  roles: string[];
}

@Component({
  selector: 'app-edit-roles-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './edit-roles-dialog.component.html',
  styleUrls: ['./edit-roles-dialog.component.scss']
})
export class EditRolesDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<EditRolesDialogComponent>);
  data = inject<EditRolesDialogData>(MAT_DIALOG_DATA);

  availableRoles = ['Portfolio Manager', 'Trader', 'Risk Analyst', 'Viewer', 'Admin'];

  rolesForm = this.fb.group({
    roles: [this.data.roles, Validators.required]
  });

  onCancel() {
    this.dialogRef.close();
  }

  onSubmit() {
    if (this.rolesForm.valid) {
      this.dialogRef.close(this.rolesForm.value.roles);
    }
  }
}