import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface Company {
  id: string;
  cnpj: string;
  corporateName: string;
  tradeName: string;
  category: string;
  city: string;
  state: string;
  cceeCode: string;
  cceeProfile?: string;
}

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/v1/companies`;

  companies = signal<Company[]>([]);
  isLoading = signal<boolean>(false);

  loadCompanies(): void {
    this.isLoading.set(true);
    this.http.get<Company[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.companies.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.companies.set([]);
        this.isLoading.set(false);
      }
    });
  }
}
