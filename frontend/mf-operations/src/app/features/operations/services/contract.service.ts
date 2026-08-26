import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface Contract {
  id?: string;
  counterpartyName: string;
  type: string; // 'Compra' | 'Venda'
  submarket: string;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
  strikePrice?: number;
  optionPremium?: number;
  tenantId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:8080/api/v1.0/Contracts';

  // State Management
  private contractsSubject = new BehaviorSubject<Contract[]>([]);
  public contracts$ = this.contractsSubject.asObservable();

  constructor() {
    this.loadContracts();
  }

  loadContracts() {
    this.http.get<Contract[]>(this.apiUrl).subscribe({
      next: (data) => this.contractsSubject.next(data),
      error: (err) => console.error('Error fetching contracts:', err)
    });
  }

  createContract(contract: Contract): Observable<any> {
    return this.http.post(this.apiUrl, contract).pipe(
      tap(() => this.loadContracts()) // Refresh list after creation
    );
  }
}
