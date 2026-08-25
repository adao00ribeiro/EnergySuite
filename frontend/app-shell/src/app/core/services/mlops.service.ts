import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

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

@Injectable({
  providedIn: 'root'
})
export class MlopsService {
  private riskApiUrl = 'http://localhost:8000/api/v1/risk';

  constructor(private http: HttpClient) {}

  public getPriceForecasts(): Observable<PriceForecastPoint[]> {
    // Generate realistic forward price curve for Brazil's submarkets (ONS / PLD)
    const today = new Date();
    const forecast: PriceForecastPoint[] = [];

    const baseSE = 145.50;
    const baseS = 142.10;
    const baseNE = 138.00;
    const baseN = 135.20;

    for (let i = 0; i < 12; i++) {
      const d = new Date(today.getFullYear(), today.getMonth() + i, 1);
      const monthLabel = d.toLocaleDateString('pt-BR', { month: 'short', year: '2-digit' }).toUpperCase();
      
      const seasonalFactor = 1 + 0.12 * Math.sin((i / 12) * 2 * Math.PI);
      const noise = (Math.sin(i * 1.5) * 4);

      forecast.push({
        date: monthLabel,
        pldSE: Number((baseSE * seasonalFactor + noise).toFixed(2)),
        pldS: Number((baseS * seasonalFactor + noise * 0.9).toFixed(2)),
        pldNE: Number((baseNE * seasonalFactor + noise * 1.1).toFixed(2)),
        pldN: Number((baseN * seasonalFactor + noise * 0.8).toFixed(2)),
        modelConfidence: Number((96.5 - i * 0.8).toFixed(1))
      });
    }

    return of(forecast);
  }

  public getRiskSummary(): Observable<RiskMetricsSummary> {
    return of({
      totalExposure: 14580250.00,
      var95: 724500.00,
      var99: 1150800.00,
      stressTestLoss: 2890000.00,
      lastUpdated: new Date().toLocaleTimeString('pt-BR'),
      activeContractsCount: 24
    });
  }
}
