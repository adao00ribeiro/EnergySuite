import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

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

export interface PositionData {
  portfolioId: string;
  portfolioName: string;
  totalPurchased: number;
  totalSold: number;
  netPosition: number;
  estimatedResult: number;
  monthlyData: { month: string; purchased: number; sold: number; net: number }[];
  detailedGaps: any[];
  heatmap: any;
}

export interface SimulationResult {
  previousVolumeMwm: number;
  newVolumeMwm: number;
  volumeDelta: number;
  previousEstimatedResult: number;
  newEstimatedResult: number;
  financialDelta: number;
  copilotAnalysis: {
    recommendation: string;
    summaryText?: string;
  } | null;
}

export interface ApproveResponse {
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class PortfolioService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/portfolio`;

  getOpportunities(): Observable<Opportunity[]> {
    return this.http.get<Opportunity[]>(`${this.baseUrl}/opportunities`);
  }

  getDashboardData(): Observable<PositionData> {
    return this.http.get<any>(`${this.baseUrl}/position`).pipe(
      map(p => ({
        portfolioId: p.portfolioId,
        portfolioName: p.portfolioName,
        totalPurchased: p.totalPurchasedMwMed,
        totalSold: p.totalSoldMwMed,
        netPosition: p.netPositionMwMed,
        estimatedResult: p.estimatedResult,
        monthlyData: (p.monthlyPositions ?? []).map((m: any) => ({
          month: m.month,
          purchased: m.purchased,
          sold: m.sold,
          net: m.net
        })),
        detailedGaps: p.detailedGaps ?? [],
        heatmap: p.heatmap ?? { xAxisMonths: [], points: [], yAxisSubmarkets: [] }
      }))
    );
  }

  simulateOperation(opportunityId: string, customVolume?: number): Observable<SimulationResult> {
    return this.http.post<SimulationResult>(`${this.baseUrl}/simulate`, {
      opportunityId,
      volumeMwm: customVolume
    });
  }

  approveOperation(operationId: string, opportunityId: string, requestedVolumeMwm: number): Observable<ApproveResponse> {
    return this.http.post<ApproveResponse>(`${this.baseUrl}/approve`, {
      operationId,
      opportunityId,
      requestedVolumeMwm
    });
  }
}