import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { Subscription } from 'rxjs';
import { MlopsService, PriceForecastPoint, RiskMetricsSummary } from '../../core/services/mlops.service';
import { RiskSignalrService, RiskCalculatedEvent } from '../../core/services/risk-signalr.service';

@Component({
  selector: 'app-executive-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTableModule,
    MatTabsModule
  ],
  templateUrl: './executive-dashboard.component.html',
  styleUrls: ['./executive-dashboard.component.scss']
})
export class ExecutiveDashboardComponent implements OnInit, OnDestroy {
  public forecastData: PriceForecastPoint[] = [];
  public riskSummary!: RiskMetricsSummary;
  public realTimeEvents: RiskCalculatedEvent[] = [];
  public selectedSubmarket: 'pldSE' | 'pldS' | 'pldNE' | 'pldN' = 'pldSE';
  
  private signalrSub!: Subscription;

  constructor(
    private mlopsService: MlopsService,
    private riskSignalrService: RiskSignalrService
  ) {}

  ngOnInit(): void {
    this.mlopsService.getPriceForecasts().subscribe(data => {
      this.forecastData = data;
    });

    this.mlopsService.getRiskSummary().subscribe(data => {
      this.riskSummary = data;
    });

    this.riskSignalrService.startConnection();
    this.signalrSub = this.riskSignalrService.riskCalculated$.subscribe(event => {
      this.realTimeEvents.unshift(event);
      if (this.realTimeEvents.length > 10) {
        this.realTimeEvents.pop();
      }
      // Dynamically update total exposure
      if (this.riskSummary) {
        this.riskSummary.totalExposure += event.financialExposure;
        this.riskSummary.lastUpdated = new Date().toLocaleTimeString('pt-BR');
      }
    });
  }

  ngOnDestroy(): void {
    if (this.signalrSub) {
      this.signalrSub.unsubscribe();
    }
  }

  public setSubmarket(market: 'pldSE' | 'pldS' | 'pldNE' | 'pldN'): void {
    this.selectedSubmarket = market;
  }

  public getMaxPrice(): number {
    if (!this.forecastData.length) return 200;
    return Math.max(...this.forecastData.map(d => d[this.selectedSubmarket])) * 1.15;
  }

  public getBarHeightPercent(price: number): number {
    const max = this.getMaxPrice();
    return Math.min(100, Math.max(10, (price / max) * 100));
  }
}
