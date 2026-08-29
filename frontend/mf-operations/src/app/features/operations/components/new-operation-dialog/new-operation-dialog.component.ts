import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CompanyService, Company } from '../../../commercial-registry/services/company.service';
import { ContractService, Contract } from '../../services/contract.service';

@Component({
  selector: 'app-new-operation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  templateUrl: './new-operation-dialog.component.html',
  styleUrl: './new-operation-dialog.component.css'
})
export class NewOperationDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private companyService = inject(CompanyService);
  private contractService = inject(ContractService);
  private dialogRef = inject(MatDialogRef<NewOperationDialogComponent>);
  data = inject<{ actionType: 'Compra' | 'Venda' }>(MAT_DIALOG_DATA);

  form!: FormGroup;
  companies = signal<Company[]>([]);
  submarkets = ['SE_CO', 'SUL', 'NORDESTE', 'NORTE'];
  isSaving = false;

  ngOnInit(): void {
    this.form = this.fb.group({
      counterparty: ['', Validators.required],
      type: [this.data.actionType === 'Compra' ? 'Purchase' : 'Sale', Validators.required],
      submarket: ['SE_CO', Validators.required],
      volumeMwMed: [null, [Validators.required, Validators.min(1)]],
      price: [null, [Validators.required, Validators.min(0)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required]
    });

    this.companyService.loadCompanies();
    this.companies = this.companyService.companies;
  }

  onSave(): void {
    if (this.form.invalid) return;

    this.isSaving = true;
    const val = this.form.value;

    const selectedCompany = this.companyService.companies().find(
      c => c.id === val.counterparty
    );

    const contract: Contract = {
      counterpartyName: selectedCompany ? selectedCompany.tradeName : '',
      type: val.type,
      submarket: val.submarket,
      volumeMwMed: val.volumeMwMed,
      price: val.price,
      startDate: val.startDate,
      endDate: val.endDate
    };

    this.contractService.createContract(contract).subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogRef.close(true);
      },
      error: (err) => {
        console.error('Error creating contract:', err);
        this.isSaving = false;
      }
    });
  }
}
