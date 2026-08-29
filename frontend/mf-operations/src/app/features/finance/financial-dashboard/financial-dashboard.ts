import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FinanceService, FinancialSettlementItem, OperationToBillItem } from '../services/finance.service';

@Component({
  selector: 'app-financial-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatIconModule,
    MatButtonModule,
    MatTabsModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './financial-dashboard.html',
  styleUrl: './financial-dashboard.scss'
})
export class FinancialDashboardComponent implements OnInit {
  private financeService = inject(FinanceService);

  displayedColumns: string[] = ['counterpartyName', 'referenceMonth', 'type', 'amount', 'dueDate', 'status', 'actions'];
  operationsColumns: string[] = ['counterpartyName', 'operationType', 'volumeMwMed', 'price', 'period', 'actions'];

  openSettlements = this.financeService.openSettlements;
  operationsToBill = this.financeService.operationsToBill;
  totalPayable = () => this.financeService.totals().totalPayable;
  totalReceivable = () => this.financeService.totals().totalReceivable;
  netBalance = () => this.financeService.totals().netBalance;
  isLoading = this.financeService.isLoading;

  ngOnInit(): void {
    this.financeService.loadDashboard();
  }

  executeNetting(element: FinancialSettlementItem): void {
    this.financeService.executeOffset(element.counterpartyId, element.referenceMonth);
  }

  generateBilling(operation: OperationToBillItem): void {
    this.financeService.generateBilling(operation);
  }
}
