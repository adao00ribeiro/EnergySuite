import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute } from '@angular/router';
import { ContractService, ContractDetails, ContractAmendment } from '../services/contract.service';

@Component({
  selector: 'app-contract-details',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './contract-details.html',
  styleUrl: './contract-details.scss'
})
export class ContractDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private contractService = inject(ContractService);

  contract = signal<ContractDetails | null>(null);
  isLoading = signal<boolean>(true);
  amendmentsColumns: string[] = ['version', 'description', 'effectiveDate', 'previousPrice', 'newPrice'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.contractService.getById(id).subscribe({
        next: (data) => {
          this.contract.set(data);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    } else {
      this.isLoading.set(false);
    }
  }
}
