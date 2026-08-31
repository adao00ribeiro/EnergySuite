import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { IndicatorCardsComponent } from './components/indicator-cards/indicator-cards.component';
import { PositionChartComponent } from './components/position-chart/position-chart.component';
import { PositionGridComponent } from './components/position-grid/position-grid.component';
import { HeatmapChartComponent } from './components/heatmap-chart/heatmap-chart.component';
import { PortfolioService } from '../../core/services/portfolio.service';
import { NewOpportunityDialogComponent } from '../opportunities/components/new-opportunity-dialog/new-opportunity-dialog.component';

interface PortfolioData {
  totalPurchased: number;
  totalSold: number;
  netPosition: number;
  estimatedResult: number;
  monthlyData: any[];
  detailedGaps: any[];
  heatmap: any;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTabsModule,
    MatDialogModule,
    MatSnackBarModule,
    IndicatorCardsComponent,
    PositionChartComponent,
    PositionGridComponent,
    HeatmapChartComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  portfolioData = signal<any | null>(null);
  selectedPortfolio = signal<string>('portfolio_1');
  portfolioService = inject(PortfolioService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  ngOnInit() {
    this.loadData();
  }

  onPortfolioChange(portfolioId: string) {
    this.selectedPortfolio.set(portfolioId);
    this.loadData();
  }

  private loadData() {
    this.portfolioService.getDashboardData().subscribe({
      next: (data: any) => {
        this.portfolioData.set(data);
      },
      error: (err) => {
        console.error('Error fetching dashboard data:', err);
      }
    });
  }

  onNewOpportunity(): void {
    const dialogRef = this.dialog.open(NewOpportunityDialogComponent, {
      width: '580px',
      panelClass: 'glass-panel-dialog'
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        this.loadData();
        this.snackBar.open(`Oportunidade "${res.title}" criada com sucesso!`, 'OK', { duration: 4000 });
      }
    });
  }
}

