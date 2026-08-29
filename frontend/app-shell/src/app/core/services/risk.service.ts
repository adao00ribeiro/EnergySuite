import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CounterpartyRisk {
  counterparty_name: string;
  financial_exposure: number;
  mark_to_market: number;
}

@Injectable({
  providedIn: 'root'
})
export class RiskService {
  private http = inject(HttpClient);
  // URL base para o Risk Service (Python FastAPI)
  private readonly apiUrl = `${environment.riskApiUrl}/metrics/portfolio`;

  public getPortfolioRisk(): Observable<CounterpartyRisk[]> {
    return this.http.get<CounterpartyRisk[]>(this.apiUrl);
  }
}
