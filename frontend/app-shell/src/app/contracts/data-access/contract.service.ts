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

import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ContractService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/contracts`;

  // Signals para reatividade local
  public readonly contracts = signal<Contract[]>([]);
  public readonly isLoading = signal<boolean>(false);
  public readonly error = signal<string | null>(null);

  public loadContracts() {
    this.isLoading.set(true);
    this.error.set(null);
    
    this.http.get<Contract[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.contracts.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar contratos:', err);
        this.error.set('Falha ao carregar contratos.');
        this.isLoading.set(false);
      }
    });
  }

  public createContract(payload: CreateContractPayload) {
    this.isLoading.set(true);
    this.error.set(null);
    return this.http.post<{id: string}>(this.apiUrl, payload);
  }
}
