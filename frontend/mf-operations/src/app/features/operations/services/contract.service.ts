import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface Contract {
  id?: string;
  counterpartyName: string;
  type: string;
  submarket: string;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
  strikePrice?: number;
  optionPremium?: number;
  tenantId?: string;
}

export interface ContractAmendment {
  id: string;
  version: number;
  description: string;
  effectiveDate: string;
  previousPrice: number;
  newPrice: number;
  createdAt: string;
}

export interface ContractDetails {
  id: string;
  counterpartyName: string;
  type: string;
  submarket: string;
  volumeMwMed: number;
  price: number;
  startDate: string;
  endDate: string;
  version: number;
  priceIndexType: string;
  flexibilityMargin: number;
  amendments: ContractAmendment[];
}

@Injectable({ providedIn: 'root' })
export class ContractService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/v1/contracts`;

  contracts = signal<Contract[]>([]);
  isLoading = signal<boolean>(false);

  loadContracts(): void {
    this.isLoading.set(true);
    this.http.get<Contract[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.contracts.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.contracts.set([]);
        this.isLoading.set(false);
      }
    });
  }

  createContract(contract: Contract): Observable<unknown> {
    return this.http.post(this.apiUrl, contract).pipe(
      tap(() => this.loadContracts())
    );
  }

  getById(id: string): Observable<ContractDetails> {
    return this.http.get<ContractDetails>(`${this.apiUrl}/${id}`);
  }
}
