import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface ApprovalItem {
  id: string;
  ticketRef: string;
  type: string;
  counterparty: string;
  volume: number;
  price: number;
  requestedBy: string;
  requestedAt: Date;
}

@Component({
  selector: 'app-approval-center',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './approval-center.html',
  styleUrl: './approval-center.scss'
})
export class ApprovalCenterComponent implements OnInit {
  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'details', 'requested', 'actions'];
  dataSource = signal<ApprovalItem[]>([]);

  ngOnInit(): void {
    this.dataSource.set([
      { id: '2', ticketRef: 'TKT-2023-002', type: 'Sale', counterparty: 'Votener SA', volume: 10.0, price: 135.0, requestedBy: 'João Trader', requestedAt: new Date() }
    ]);
  }

  approve(id: string) {
    console.log('Approve', id);
  }

  reject(id: string) {
    console.log('Reject', id);
  }
}
