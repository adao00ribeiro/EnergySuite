import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Contract {
  id: string;
  counterpartyName: string;
  type: number;
  submarket: number;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
  createdAt: string;
}

export interface CreateContractPayload {
  counterpartyName: string;
  type: number;
  submarket: number;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/contracts'; // TODO: Move to environment

  // Signals para reatividade local
  public readonly contracts = signal<Contract[]>([]);
  public readonly isLoading = signal<boolean>(false);
  public readonly error = signal<string | null>(null);

  // Mocks por enquanto, até implementarmos o GET na API
  public loadContracts() {
    this.isLoading.set(true);
    // Simulating API Call
    setTimeout(() => {
      this.contracts.set([
        {
          id: '1',
          counterpartyName: 'Solaris Energy',
          type: 1, // Sale
          submarket: 2, // Nordeste
          volumeMwMed: 15.5,
          price: 210.0,
          startDate: '2026-01-01',
          endDate: '2026-12-31',
          createdAt: new Date().toISOString()
        }
      ]);
      this.isLoading.set(false);
    }, 1000);
  }

  public createContract(payload: CreateContractPayload) {
    this.isLoading.set(true);
    this.error.set(null);
    return this.http.post<{id: string}>(this.apiUrl, payload);
  }
}
