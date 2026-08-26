import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface OperationItem {
  id: string;
  ticketRef: string;
  type: string;
  counterparty: string;
  volume: number;
  price: number;
  state: string;
}

@Component({
  selector: 'app-tickets-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './tickets-list.html',
  styleUrl: './tickets-list.scss'
})
export class TicketsListComponent implements OnInit {
  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'volume', 'price', 'state', 'actions'];
  dataSource = signal<OperationItem[]>([]);

  ngOnInit(): void {
    // Mock data
    this.dataSource.set([
      { id: '1', ticketRef: 'TKT-2023-001', type: 'Purchase', counterparty: 'Matrix Energia S/A', volume: 15.5, price: 120.5, state: 'Draft' },
      { id: '2', ticketRef: 'TKT-2023-002', type: 'Sale', counterparty: 'Votener SA', volume: 10.0, price: 135.0, state: 'PendingApproval' }
    ]);
  }
}
