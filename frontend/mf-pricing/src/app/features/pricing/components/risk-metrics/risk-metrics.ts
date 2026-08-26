import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface RiskMetric {
  title: string;
  value: string;
  trend: 'up' | 'down' | 'neutral';
  trendValue: string;
  description: string;
}

@Component({
  selector: 'app-risk-metrics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './risk-metrics.html',
  styleUrl: './risk-metrics.css'
})
export class RiskMetricsComponent implements OnInit {
  @Input() metrics: RiskMetric[] = [
    {
      title: 'Global VaR (95%)',
      value: 'R$ 14.2M',
      trend: 'down',
      trendValue: '-1.5%',
      description: 'Daily Value at Risk across all portfolios'
    },
    {
      title: 'Mark-to-Market (MtM)',
      value: 'R$ 285.5M',
      trend: 'up',
      trendValue: '+4.2%',
      description: 'Total current market exposure'
    },
    {
      title: 'Implied Volatility',
      value: '18.4%',
      trend: 'neutral',
      trendValue: '0.0%',
      description: 'Weighted average volatility index'
    }
  ];

  ngOnInit(): void {}
}
