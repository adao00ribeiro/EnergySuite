import { Component, OnInit, AfterViewInit, inject, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { OperationsService, OperationListItem } from '../services/operations.service';
import { NewOperationDialogComponent } from '../components/new-operation-dialog/new-operation-dialog.component';

@Component({
  selector: 'app-tickets-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './tickets-list.html',
  styleUrl: './tickets-list.scss'
})
export class TicketsListComponent implements OnInit, AfterViewInit {
  private operationsService = inject(OperationsService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'volume', 'price', 'state', 'actions'];
  dataSource = new MatTableDataSource<OperationListItem>([]);

  constructor() {
    effect(() => {
      const data = this.operationsService.operations();
      this.dataSource.data = data;
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  ngOnInit(): void {
    this.operationsService.loadOperations();
  }

  onNewOperation(): void {
    const dialogRef = this.dialog.open(NewOperationDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      panelClass: 'glass-panel-dialog',
      data: { actionType: 'Compra' }
    });

    dialogRef.afterClosed().subscribe((saved) => {
      if (saved) {
        this.operationsService.loadOperations();
        this.snackBar.open('Operação registrada com sucesso!', 'OK', { duration: 4000 });
      }
    });
  }

  onEditOperation(id: string): void {
    const dialogRef = this.dialog.open(NewOperationDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      panelClass: 'glass-panel-dialog',
      data: { actionType: 'Venda' }
    });

    dialogRef.afterClosed().subscribe((saved) => {
      if (saved) {
        this.operationsService.loadOperations();
        this.snackBar.open(`Operação #${id} atualizada com sucesso!`, 'OK', { duration: 4000 });
      }
    });
  }
}

