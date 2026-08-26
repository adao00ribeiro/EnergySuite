import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AssetAllocationComponent } from '../components/asset-allocation/asset-allocation';
import { EnergyBalanceChartComponent } from '../components/energy-balance-chart/energy-balance-chart';

@Component({
  selector: 'app-portfolio-dashboard',
  standalone: true,
  imports: [CommonModule, AssetAllocationComponent, EnergyBalanceChartComponent],
  templateUrl: './portfolio-dashboard.html',
  styleUrl: './portfolio-dashboard.scss'
})
export class PortfolioDashboardComponent {
  currentDate = new Date();
}
