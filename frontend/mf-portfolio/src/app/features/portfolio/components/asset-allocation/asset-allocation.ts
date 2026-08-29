import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { PortfolioService } from '../../../../core/services/portfolio.service';

interface AllocationItem {
  title: string;
  value: string;
  percentage: number;
  color: string;
}

@Component({
  selector: 'app-asset-allocation',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './asset-allocation.html',
  styleUrl: './asset-allocation.scss'
})
export class AssetAllocationComponent implements OnInit {
  private portfolioService = inject(PortfolioService);

  allocations: AllocationItem[] = [];
  isEmpty = signal<boolean>(false);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.portfolioService.getDashboardData().subscribe({
      next: (data) => {
        this.isLoading.set(false);

        const purchased = data.totalPurchased || 0;
        const sold = data.totalSold || 0;
        const net = data.netPosition || 0;
        const total = purchased + sold;

        if (total <= 0) {
          this.isEmpty.set(true);
          this.allocations = [];
          return;
        }
        this.isEmpty.set(false);
        this.allocations = [
          { title: 'Compras', value: `${purchased} MWm`, percentage: Math.round((purchased / total) * 100), color: 'blue' },
          { title: 'Vendas', value: `${sold} MWm`, percentage: Math.round((sold / total) * 100), color: 'slate' },
          { title: 'Posição Líquida', value: `${net} MWm`, percentage: Math.min(100, Math.round((Math.abs(net) / total) * 100)), color: 'rose' }
        ];
      },
      error: (err) => {
        console.error('Erro ao carregar alocação do portfólio:', err);
        this.isLoading.set(false);
        this.isEmpty.set(true);
        this.allocations = [];
      }
    });
  }
}