import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CompanyService, CreateCompanyPayload } from '../../services/company.service';

@Component({
  selector: 'app-new-company-dialog',
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
  templateUrl: './new-company-dialog.component.html',
  styleUrl: './new-company-dialog.component.css'
})
export class NewCompanyDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private companyService = inject(CompanyService);
  private dialogRef = inject(MatDialogRef<NewCompanyDialogComponent>);

  form!: FormGroup;
  isSaving = false;

  ngOnInit(): void {
    this.form = this.fb.group({
      cnpj: ['', Validators.required],
      corporateName: ['', Validators.required],
      tradeName: ['', Validators.required],
      category: ['Contraparte', Validators.required],
      zipCode: [''],
      street: [''],
      number: [''],
      complement: [''],
      neighborhood: [''],
      city: [''],
      state: ['']
    });
  }

  onSave(): void {
    if (this.form.invalid) return;

    this.isSaving = true;
    const val = this.form.value;

    const payload: CreateCompanyPayload = {
      cnpj: val.cnpj,
      corporateName: val.corporateName,
      tradeName: val.tradeName,
      category: val.category,
      zipCode: val.zipCode,
      street: val.street,
      number: val.number,
      complement: val.complement,
      neighborhood: val.neighborhood,
      city: val.city,
      state: val.state
    };

    this.companyService.createCompany(payload).subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogRef.close(true);
      },
      error: (err) => {
        console.error('Error creating company:', err);
        this.isSaving = false;
      }
    });
  }
}
