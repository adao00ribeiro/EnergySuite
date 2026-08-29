import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, switchMap, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RiskService, CounterpartyRisk } from './risk.service';

export interface PriceForecastPoint {
  date: string;
  pldSE: number;
  pldS: number;
  pldNE: number;
  pldN: number;
  modelConfidence: number;
}

export interface RiskMetricsSummary {
  totalExposure: number;
  var95: number;
  var99: number;
  stressTestLoss: number;
  lastUpdated: string;
  activeContractsCount: number;
}

interface StudyResultResponse {
  results: Array<{ month: string; pldSE: number; pldS: number; pldNE: number; pldN: number }>;
}

@Injectable({
  providedIn: 'root'
})
export class MlopsService {
  private http = inject(HttpClient);
  private riskService = inject(RiskService);

  public getPriceForecasts(): Observable<PriceForecastPoint[]> {
    return this.http.get<Array<{ id: string; state: string }>>(`${environment.apiUrl}/prospect/studies`).pipe(
      switchMap(studies => {
        const latest = (Array.isArray(studies) ? studies : []).find(s => s.state === 'Completed');

        if (!latest) {
          return of([]);
        }

        return this.http.get<StudyResultResponse>(`${environment.apiUrl}/prospect/studies/${latest.id}/results`).pipe(
          map(response => {
            const results = response?.results ?? [];

            return results.map(r => ({
              date: r.month,
              pldSE: r.pldSE,
              pldS: r.pldS,
              pldNE: r.pldNE,
              pldN: r.pldN,
              modelConfidence: 100
            }));
          })
        );
      })
    );
  }

  public getRiskSummary(): Observable<RiskMetricsSummary> {
    return this.riskService.getPortfolioRisk().pipe(
      map(rows => {
        const portfolio: CounterpartyRisk[] = Array.isArray(rows) ? rows : [];
        const totalExposure = portfolio.reduce((sum, row) => sum + row.financial_exposure, 0);

        return {
          totalExposure,
          var95: 0,
          var99: 0,
          stressTestLoss: 0,
          lastUpdated: new Date().toLocaleTimeString('pt-BR'),
          activeContractsCount: portfolio.length
        };
      })
    );
  }
}