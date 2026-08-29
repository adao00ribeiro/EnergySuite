import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { OperationsService, OperationListItem } from '../services/operations.service';

@Component({
  selector: 'app-tickets-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './tickets-list.html',
  styleUrl: './tickets-list.scss'
})
export class TicketsListComponent implements OnInit {
  private operationsService = inject(OperationsService);

  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'volume', 'price', 'state', 'actions'];
  dataSource = this.operationsService.operations;

  ngOnInit(): void {
    this.operationsService.loadOperations();
  }

  onNewOperation() {
    alert('Funcionalidade "Nova Operação" será implementada na próxima sprint!');
  }

  onEditOperation(id: string) {
    alert(`Editando operação ID: ${id}`);
  }
}
