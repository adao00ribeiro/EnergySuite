import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ContractService } from '../../data-access/contract.service';

@Component({
  selector: 'app-contract-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './contract-list.component.html',
  styleUrls: ['./contract-list.component.scss']
})
export class ContractListComponent implements OnInit {
  public contractService = inject(ContractService);
  public displayedColumns: string[] = ['counterpartyName', 'volumeMwMed', 'price', 'period'];

  ngOnInit() {
    this.contractService.loadContracts();
  }
}
