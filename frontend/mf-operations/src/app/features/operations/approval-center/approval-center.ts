import { Component, OnInit, AfterViewInit, inject, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { OperationsService, OperationListItem } from '../services/operations.service';

@Component({
  selector: 'app-approval-center',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './approval-center.html',
  styleUrl: './approval-center.scss'
})
export class ApprovalCenterComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'details', 'actions'];

  private operationsService = inject(OperationsService);
  private snackBar = inject(MatSnackBar);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  dataSource = new MatTableDataSource<OperationListItem>([]);

  constructor() {
    effect(() => {
      const pending = this.operationsService.operations().filter(op => op.state === 'PendingApproval');
      this.dataSource.data = pending;
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  ngOnInit(): void {
    this.operationsService.loadOperations();
  }

  approve(id: string) {
    this.operationsService.changeState(id, 'Approved').subscribe({
      next: () => {
        this.snackBar.open('Operação aprovada com sucesso.', 'Fechar', { duration: 3000 });
        this.operationsService.loadOperations();
      },
      error: (err) => {
        console.error('Erro ao aprovar operação', err);
        this.snackBar.open('Falha ao aprovar a operação.', 'Fechar', { duration: 5000, panelClass: ['warn-snackbar'] });
      }
    });
  }

  reject(id: string) {
    this.operationsService.changeState(id, 'Inactive').subscribe({
      next: () => {
        this.snackBar.open('Operação rejeitada e marcada como inativa.', 'Fechar', { duration: 3000 });
        this.operationsService.loadOperations();
      },
      error: (err) => {
        console.error('Erro ao rejeitar operação', err);
        this.snackBar.open('Falha ao rejeitar a operação.', 'Fechar', { duration: 5000, panelClass: ['warn-snackbar'] });
      }
    });
  }
}