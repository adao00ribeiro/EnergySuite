import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface ContractAmendment {
  id: string;
  version: number;
  description: string;
  effectiveDate: Date;
  previousPrice: number;
  newPrice: number;
  createdAt: Date;
}

export interface ContractDetails {
  id: string;
  counterpartyName: string;
  type: string;
  submarket: string;
  volumeMwMed: number;
  price: number;
  startDate: Date;
  endDate: Date;
  version: number;
  priceIndexType: string;
  flexibilityMargin: number;
  amendments: ContractAmendment[];
}

@Component({
  selector: 'app-contract-details',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule],
  templateUrl: './contract-details.html',
  styleUrl: './contract-details.scss'
})
export class ContractDetailsComponent implements OnInit {
  contract = signal<ContractDetails | null>(null);
  amendmentsColumns: string[] = ['version', 'description', 'effectiveDate', 'previousPrice', 'newPrice'];

  ngOnInit(): void {
    // Mock Data for the newly implemented Readjustments
    this.contract.set({
      id: '00000000-0000-0000-0000-000000000001',
      counterpartyName: 'Matrix Energia S/A',
      type: 'Purchase',
      submarket: 'SE_CO',
      volumeMwMed: 15.5,
      price: 110.5,
      startDate: new Date('2025-01-01'),
      endDate: new Date('2030-12-31'),
      version: 2,
      priceIndexType: 'IPCA',
      flexibilityMargin: 0.1,
      amendments: [
        {
          id: 'a1',
          version: 2,
          description: 'Reajuste IPCA 10%',
          effectiveDate: new Date('2026-01-01'),
          previousPrice: 100.0,
          newPrice: 110.5,
          createdAt: new Date('2026-01-01T10:00:00')
        }
      ]
    });
  }
}
