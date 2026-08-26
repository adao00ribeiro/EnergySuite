import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';

export interface FinancialSettlementItem {
  id: string;
  counterpartyName: string;
  referenceMonth: string;
  type: string;
  amount: number;
  dueDate: Date;
  status: string;
}

export interface OperationToBillItem {
  id: string;
  counterpartyName: string;
  operationType: string;
  volumeMwMed: number;
  price: number;
  startDate: Date;
  endDate: Date;
}

@Component({
  selector: 'app-financial-dashboard',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule, MatTabsModule],
  templateUrl: './financial-dashboard.html',
  styleUrl: './financial-dashboard.scss'
})
export class FinancialDashboardComponent implements OnInit {
  displayedColumns: string[] = ['counterpartyName', 'referenceMonth', 'type', 'amount', 'dueDate', 'status', 'actions'];
  operationsColumns: string[] = ['counterpartyName', 'operationType', 'volumeMwMed', 'price', 'period', 'actions'];
  
  openSettlements = signal<FinancialSettlementItem[]>([]);
  operationsToBill = signal<OperationToBillItem[]>([]);
  
  // Dashboard Metrics
  totalPayable = signal<number>(0);
  totalReceivable = signal<number>(0);
  netBalance = signal<number>(0);

  ngOnInit(): void {
    // Mock Data simulating the newly implemented Billing and Financial Settlements
    const mockData: FinancialSettlementItem[] = [
      { id: '1', counterpartyName: 'Matrix Energia S/A', referenceMonth: '2026-08', type: 'Payable', amount: 150000.50, dueDate: new Date('2026-09-15'), status: 'Open' },
      { id: '2', counterpartyName: 'Matrix Energia S/A', referenceMonth: '2026-08', type: 'Receivable', amount: 80500.00, dueDate: new Date('2026-09-15'), status: 'Open' },
      { id: '3', counterpartyName: 'Votener SA', referenceMonth: '2026-08', type: 'Receivable', amount: 250000.00, dueDate: new Date('2026-09-15'), status: 'Open' },
      { id: '4', counterpartyName: 'Casa dos Ventos', referenceMonth: '2026-08', type: 'Payable', amount: 10000.00, dueDate: new Date('2026-09-15'), status: 'Open' }
    ];
    
    const mockOperations: OperationToBillItem[] = [
      { id: 'op1', counterpartyName: 'Matrix Energia S/A', operationType: 'Purchase', volumeMwMed: 15.5, price: 120.5, startDate: new Date('2026-08-01'), endDate: new Date('2026-08-31') },
      { id: 'op2', counterpartyName: 'Votener SA', operationType: 'Sale', volumeMwMed: 10.0, price: 135.0, startDate: new Date('2026-08-01'), endDate: new Date('2026-08-31') }
    ];
    
    this.openSettlements.set(mockData);
    this.operationsToBill.set(mockOperations);
    this.calculateMetrics(mockData);
  }
  
  calculateMetrics(data: FinancialSettlementItem[]) {
    const payable = data.filter(d => d.type === 'Payable').reduce((sum, current) => sum + current.amount, 0);
    const receivable = data.filter(d => d.type === 'Receivable').reduce((sum, current) => sum + current.amount, 0);
    
    this.totalPayable.set(payable);
    this.totalReceivable.set(receivable);
    this.netBalance.set(receivable - payable);
  }

  executeNetting(counterpartyName: string) {
    // Simulates the backend ExecuteAccountOffsetCommand being triggered
    console.log(`Executing Netting (Account Offset) for ${counterpartyName}...`);
    // In a real scenario, this calls the API and then refreshes the data.
  }

  generateBilling(operation: OperationToBillItem) {
    console.log(`Generating Billing for Operation ${operation.id} (${operation.counterpartyName})...`);
    // Simulates the backend GenerateBillingCommand being triggered
  }
}
