import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

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
    MatProgressBarModule
  ],
  templateUrl: './opportunities-book.component.html',
  styleUrls: ['./opportunities-book.component.scss']
})
export class OpportunitiesBookComponent implements OnInit {
  displayedColumns: string[] = ['score', 'name', 'type', 'target', 'volume', 'spread', 'actions'];
  dataSource: MatTableDataSource<Opportunity> = new MatTableDataSource();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit() {
    this.loadMockOpportunities();
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
