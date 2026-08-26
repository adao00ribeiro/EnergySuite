import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RiskMetricsComponent } from '../components/risk-metrics/risk-metrics';
import { ForwardCurveChartComponent } from '../components/forward-curve-chart/forward-curve-chart';

@Component({
  selector: 'app-pricing-dashboard',
  standalone: true,
  imports: [CommonModule, RiskMetricsComponent, ForwardCurveChartComponent],
  templateUrl: './pricing-dashboard.html',
  styleUrl: './pricing-dashboard.css'
})
export class PricingDashboardComponent {
  currentDate = new Date();
}
