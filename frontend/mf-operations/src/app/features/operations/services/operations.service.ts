import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface OperationListItem {
  id: string;
  ticketId: string;
  ticketRef: string;
  type: string;
  counterparty: string;
  counterpartyName: string;
  volume: number;
  volumeMwMed: number;
  price: number;
  state: string;
}

interface OperationApiItem {
  id: string;
  ticketId: string;
  portfolioId: string;
  counterpartyId: string;
  type: string;
  state: string;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
  ticketRef: string;
  counterpartyName: string;
}

@Injectable({ providedIn: 'root' })
export class OperationsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/operations`;

  operations = signal<OperationListItem[]>([]);
  isLoading = signal<boolean>(false);

  loadOperations(): void {
    this.isLoading.set(true);
    this.http.get<OperationApiItem[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.operations.set(data.map(op => ({
          id: op.id,
          ticketId: op.ticketId,
          ticketRef: op.ticketRef,
          type: op.type,
          counterparty: op.counterpartyName,
          counterpartyName: op.counterpartyName,
          volume: op.volumeMwMed,
          volumeMwMed: op.volumeMwMed,
          price: op.price,
          state: op.state
        })));
        this.isLoading.set(false);
      },
      error: () => {
        this.operations.set([]);
        this.isLoading.set(false);
      }
    });
  }

  changeState(operationId: string, newState: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${operationId}/state`, { newState });
  }
}
