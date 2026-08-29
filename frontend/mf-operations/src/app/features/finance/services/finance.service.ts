import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface FinancialSettlementItem {
  id: string;
  counterpartyId: string;
  counterpartyName: string;
  referenceMonth: string;
  type: 'Payable' | 'Receivable';
  amount: number;
  dueDate: string;
  status: string;
}

export interface OperationToBillItem {
  id: string;
  counterpartyId: string;
  counterpartyName: string;
  operationType: 'Purchase' | 'Sale';
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
}

export interface FinanceTotals {
  totalPayable: number;
  totalReceivable: number;
  netBalance: number;
}

export interface FinanceDashboardResponse {
  openSettlements: FinancialSettlementItem[];
  operationsToBill: OperationToBillItem[];
  totals: FinanceTotals;
}

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/v1/finance`;

  openSettlements = signal<FinancialSettlementItem[]>([]);
  operationsToBill = signal<OperationToBillItem[]>([]);
  totals = signal<FinanceTotals>({ totalPayable: 0, totalReceivable: 0, netBalance: 0 });
  isLoading = signal<boolean>(false);

  loadDashboard(): void {
    this.isLoading.set(true);
    this.http.get<FinanceDashboardResponse>(`${this.apiUrl}/dashboard`).subscribe({
      next: (data) => {
        this.openSettlements.set(data.openSettlements);
        this.operationsToBill.set(data.operationsToBill);
        this.totals.set(data.totals);
        this.isLoading.set(false);
      },
      error: () => {
        this.openSettlements.set([]);
        this.operationsToBill.set([]);
        this.totals.set({ totalPayable: 0, totalReceivable: 0, netBalance: 0 });
        this.isLoading.set(false);
      }
    });
  }

  executeOffset(counterpartyId: string, referenceMonth: string): void {
    this.isLoading.set(true);
    this.http.post(`${this.apiUrl}/offset`, { counterpartyId, referenceMonth }).subscribe({
      next: () => this.loadDashboard(),
      error: () => this.isLoading.set(false)
    });
  }

  generateBilling(operation: OperationToBillItem): void {
    const referenceMonth = operation.startDate.substring(0, 7);
    this.isLoading.set(true);
    this.http.post(`${this.apiUrl}/billings`, {
      operationId: operation.id,
      referenceMonth,
      calculatedVolume: operation.volumeMwMed,
      appliedPrice: operation.price,
      taxesAmount: 0
    }).subscribe({
      next: () => this.loadDashboard(),
      error: () => this.isLoading.set(false)
    });
  }
}
