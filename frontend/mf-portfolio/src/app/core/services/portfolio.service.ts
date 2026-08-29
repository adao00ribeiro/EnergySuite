import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Opportunity {
  id: string;
  name: string;
  type: string;
  strategyName: string;
  suggestedVolumeMwm: number;
  estimatedSpread: number;
  score: number;
  targetMonth: string;
  targetSubmarket: string;
}

export interface DashboardData {
  totalExposureMwm: number;
  m2mValue: number;
  activeStrategies: number;
  opportunitiesCount: number;
  exposureHistory: { date: string; value: number }[];
  pnlHistory: { date: string; value: number }[];
  marketPrices: { submarket: string; price: number }[];
}

export interface SimulationResult {
  success: boolean;
  message: string;
  newEstimatedResult: number;
}

@Injectable({
  providedIn: 'root'
})
export class PortfolioService {
  private http = inject(HttpClient);
  private baseUrl = '/api/v1/portfolio';

  getOpportunities(): Observable<Opportunity[]> {
    return this.http.get<Opportunity[]>(`${this.baseUrl}/opportunities`);
  }

  getDashboardData(): Observable<DashboardData> {
    return this.http.get<DashboardData>(`${this.baseUrl}/dashboard`);
  }

  simulateOperation(opportunityId: string, customVolume?: number): Observable<SimulationResult> {
    return this.http.post<SimulationResult>(`${this.baseUrl}/simulate`, {
      opportunityId,
      volumeMwm: customVolume
    });
  }
}
