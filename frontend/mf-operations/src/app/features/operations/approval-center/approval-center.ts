import { Component, OnInit, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { OperationsService } from '../services/operations.service';

@Component({
  selector: 'app-approval-center',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './approval-center.html',
  styleUrl: './approval-center.scss'
})
export class ApprovalCenterComponent implements OnInit {
  displayedColumns: string[] = ['ticketRef', 'type', 'counterparty', 'details', 'actions'];

  private operationsService = inject(OperationsService);
  private snackBar = inject(MatSnackBar);

  pendingItems = computed(() =>
    this.operationsService.operations().filter(op => op.state === 'PendingApproval')
  );

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