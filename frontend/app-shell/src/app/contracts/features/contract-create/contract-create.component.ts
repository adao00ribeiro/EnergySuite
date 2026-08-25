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
    type: ['Purchase', Validators.required],
    submarket: ['SE_CO', Validators.required],
    volumeMwMed: [0, [Validators.required, Validators.min(0.1)]],
    price: [0, [Validators.required, Validators.min(0.1)]],
    startDate: [new Date(), Validators.required],
    endDate: [new Date(), Validators.required],
    strikePrice: [{ value: 0, disabled: true }, [Validators.min(0.1)]]
  });

  public contractTypes = [
    { value: 'Purchase', label: 'Compra' },
    { value: 'Sale', label: 'Venda' },
    { value: 'Swap', label: 'Swap de Preço' },
    { value: 'OptionCall', label: 'Opção de Compra (Call)' },
    { value: 'OptionPut', label: 'Opção de Venda (Put)' }
  ];

  public submarkets = [
    { value: 'SE_CO', label: 'Sudeste/Centro-Oeste' },
    { value: 'SUL', label: 'Sul' },
    { value: 'NORDESTE', label: 'Nordeste' },
    { value: 'NORTE', label: 'Norte' }
  ];

  ngOnInit() {
    this.contractForm.controls.type.valueChanges.subscribe(type => {
      const strikeControl = this.contractForm.controls.strikePrice;
      if (type === 'OptionCall' || type === 'OptionPut') {
        strikeControl.enable();
        strikeControl.setValidators([Validators.required, Validators.min(0.1)]);
      } else {
        strikeControl.disable();
        strikeControl.clearValidators();
      }
      strikeControl.updateValueAndValidity();
    });
  }

  onSubmit() {
    if (this.contractForm.valid) {
      const formValue = this.contractForm.getRawValue();
      const payload: CreateContractPayload = {
        ...formValue,
        strikePrice: formValue.type.startsWith('Option') ? formValue.strikePrice : undefined,
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
