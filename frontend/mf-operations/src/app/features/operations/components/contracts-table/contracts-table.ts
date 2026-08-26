import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContractService, Contract } from '../../services/contract.service';

@Component({
  selector: 'app-contracts-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contracts-table.html',
  styleUrl: './contracts-table.css'
})
export class ContractsTableComponent implements OnInit {
  private contractService = inject(ContractService);
  contracts: Contract[] = [];

  ngOnInit(): void {
    this.contractService.contracts$.subscribe(data => {
      this.contracts = data;
    });
  }
}
