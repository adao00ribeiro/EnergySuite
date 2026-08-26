import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Contract {
  id: string;
  counterparty: string;
  type: 'Compra' | 'Venda';
  volumeMWm: number;
  price: number;
  status: 'Vigente' | 'Encerrado' | 'Pendente';
}

@Component({
  selector: 'app-contracts-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contracts-table.html',
  styleUrl: './contracts-table.css'
})
export class ContractsTableComponent implements OnInit {
  contracts: Contract[] = [
    { id: 'CTR-2026-001', counterparty: 'Votener Energia', type: 'Compra', volumeMWm: 15.5, price: 125.40, status: 'Vigente' },
    { id: 'CTR-2026-002', counterparty: 'Matrix Energia', type: 'Venda', volumeMWm: 30.0, price: 132.00, status: 'Vigente' },
    { id: 'CTR-2026-003', counterparty: 'Eletrobras', type: 'Compra', volumeMWm: 5.0, price: 110.50, status: 'Pendente' },
    { id: 'CTR-2025-899', counterparty: 'CPFL', type: 'Venda', volumeMWm: 10.0, price: 95.00, status: 'Encerrado' },
  ];

  ngOnInit(): void {}
}
