import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

export interface RiskMetric {
  title: string;
  value: string;
  trend: 'up' | 'down' | 'neutral';
  trendValue: string;
  description: string;
}

@Component({
  selector: 'app-risk-metrics',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './risk-metrics.html',
  styleUrl: './risk-metrics.css'
})
export class RiskMetricsComponent {
  @Input() metrics: RiskMetric[] = [];
}
