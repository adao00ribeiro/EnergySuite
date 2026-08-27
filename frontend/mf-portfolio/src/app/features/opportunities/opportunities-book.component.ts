import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SimulationDialogComponent } from './components/simulation-dialog/simulation-dialog.component';

interface Opportunity {
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

@Component({
  selector: 'app-opportunities-book',
  standalone: true,
  imports: [
    CommonModule, 
    MatTableModule, 
    MatPaginatorModule, 
    MatSortModule, 
    MatButtonModule, 
    MatIconModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './opportunities-book.component.html',
  styleUrls: ['./opportunities-book.component.scss']
})
export class OpportunitiesBookComponent implements OnInit {
  displayedColumns: string[] = ['score', 'name', 'type', 'target', 'volume', 'spread', 'actions'];
  dataSource: MatTableDataSource<Opportunity> = new MatTableDataSource();
  dialog = inject(MatDialog);
  snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit() {
    this.loadMockOpportunities();
  }

  exportToCsv() {
    const data = this.dataSource.data;
    if (data.length === 0) return;
    
    const headers = ['Score', 'Nome', 'Tipo', 'Estrategia', 'Mes', 'Submercado', 'Volume_MWm', 'Spread_BRL'];
    const csvRows = [];
    csvRows.push(headers.join(','));
    
    for (const row of data) {
      const values = [
        row.score,
        `"${row.name}"`,
        row.type,
        `"${row.strategyName}"`,
        row.targetMonth,
        row.targetSubmarket,
        row.suggestedVolumeMwm,
        row.estimatedSpread
      ];
      csvRows.push(values.join(','));
    }

    const blob = new Blob([csvRows.join('\n')], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.setAttribute('hidden', '');
    a.setAttribute('href', url);
    a.setAttribute('download', 'oportunidades.csv');
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    
    this.snackBar.open('Arquivo CSV gerado com sucesso!', 'OK', { duration: 3000 });
  }

  saveFavorites() {
    // Simulando gravação no LocalStorage
    localStorage.setItem('menza_favorite_filters', JSON.stringify({ savedAt: new Date() }));
    this.snackBar.open('Filtros salvos nos favoritos!', 'OK', { duration: 3000 });
  }

  openSimulation(opportunity: Opportunity) {
    this.dialog.open(SimulationDialogComponent, {
      width: '600px',
      data: {
        opportunityId: opportunity.id,
        name: opportunity.name,
        volumeMwm: opportunity.suggestedVolumeMwm
      }
    });
  }

  loadMockOpportunities() {
    const data: Opportunity[] = [
      { 
          id: '1', 
          name: "Cobertura Déficit SE/CO (Julho)", 
          type: "Compra", 
          strategyName: "Hedge de Inverno", 
          suggestedVolumeMwm: 15.5, 
          estimatedSpread: -12.0, // Custo evitado
          score: 95, 
          targetMonth: "2026-07", 
          targetSubmarket: "SE/CO" 
      },
      { 
          id: '2', 
          name: "Desova de Excedente (Eólico NE)", 
          type: "Venda", 
          strategyName: "Venda Excedente Eólica", 
          suggestedVolumeMwm: 22.0, 
          estimatedSpread: 45.0, 
          score: 88, 
          targetMonth: "2026-10", 
          targetSubmarket: "NE" 
      },
      { 
          id: '3', 
          name: "Arbitragem Estrutural", 
          type: "Compra", 
          strategyName: "Arbitragem Sul x SE", 
          suggestedVolumeMwm: 10.0, 
          estimatedSpread: 25.5, 
          score: 72, 
          targetMonth: "2026-11", 
          targetSubmarket: "SUL" 
      }
    ];

    this.dataSource = new MatTableDataSource(data);
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  getScoreClass(score: number): string {
    if (score >= 90) return 'score-high';
    if (score >= 70) return 'score-med';
    return 'score-low';
  }

  getScoreColor(score: number): string {
    if (score >= 90) return 'primary';
    if (score >= 70) return 'accent';
    return 'warn';
  }
}
