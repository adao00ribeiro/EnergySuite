import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { NewOperationDialogComponent } from '../new-operation-dialog/new-operation-dialog.component';

@Component({
  selector: 'app-quick-action-cards',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './quick-action-cards.html',
  styleUrl: './quick-action-cards.css'
})
export class QuickActionCardsComponent {
  private router = inject(Router);
  private dialog = inject(MatDialog);

  actions = [
    { title: 'Nova Compra', description: 'Registrar contrato de energia', icon: 'plus-circle', color: 'indigo', actionType: 'Compra' as const },
    { title: 'Nova Venda', description: 'Registrar venda de energia', icon: 'arrow-up-right', color: 'violet', actionType: 'Venda' as const },
    { title: 'Liquidação', description: 'Processamento financeiro CCEE', icon: 'dollar-sign', color: 'amber', actionType: null },
  ];

  handleAction(action: { actionType: 'Compra' | 'Venda' | null }) {
    if (action.actionType) {
      this.dialog.open(NewOperationDialogComponent, {
        width: '600px',
        maxWidth: '95vw',
        panelClass: 'glass-panel-dialog',
        data: { actionType: action.actionType }
      });
    } else {
      this.router.navigate(['/finance']);
    }
  }
}
