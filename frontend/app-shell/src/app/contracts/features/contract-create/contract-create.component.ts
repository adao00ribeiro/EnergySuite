import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ContractService, CreateContractPayload } from '../../data-access/contract.service';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-contract-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  templateUrl: './contract-create.component.html',
  styleUrls: ['./contract-create.component.scss']
})
export class ContractCreateComponent {
  private fb = inject(FormBuilder);
  public contractService = inject(ContractService);
  private router = inject(Router);

  public contractForm = this.fb.nonNullable.group({
    counterpartyName: ['', Validators.required],
    type: [1, Validators.required],
    submarket: [1, Validators.required],
    volumeMwMed: [0, [Validators.required, Validators.min(0.1)]],
    price: [0, [Validators.required, Validators.min(0.1)]],
    startDate: [new Date(), Validators.required],
    endDate: [new Date(), Validators.required]
  });

  public contractTypes = [
    { value: 1, label: 'Compra' },
    { value: 2, label: 'Venda' }
  ];

  public submarkets = [
    { value: 1, label: 'Sudeste/Centro-Oeste' },
    { value: 2, label: 'Sul' },
    { value: 3, label: 'Nordeste' },
    { value: 4, label: 'Norte' }
  ];

  onSubmit() {
    if (this.contractForm.valid) {
      const formValue = this.contractForm.getRawValue();
      const payload: CreateContractPayload = {
        ...formValue,
        startDate: formValue.startDate.toISOString().split('T')[0],
        endDate: formValue.endDate.toISOString().split('T')[0]
      };

      this.contractService.createContract(payload).subscribe({
        next: () => {
          this.router.navigate(['/contracts']);
        },
        error: (err) => {
          console.error('Erro ao criar contrato:', err);
          alert('Erro ao criar contrato.');
        }
      });
    }
  }
}
