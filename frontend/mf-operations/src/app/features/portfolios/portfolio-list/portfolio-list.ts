import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface Portfolio {
  id: string;
  name: string;
  type: string;
  responsible: string;
  status: string;
}

@Component({
  selector: 'app-portfolio-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss'
})
export class PortfolioListComponent implements OnInit {
  displayedColumns: string[] = ['name', 'type', 'responsible', 'status', 'actions'];
  dataSource = signal<Portfolio[]>([]);

  ngOnInit(): void {
    this.dataSource.set([
      { id: '1', name: 'Trading SE/CO', type: 'Trading', responsible: 'João Silva', status: 'Active' },
      { id: '2', name: 'Varejo Sul', type: 'Varejo', responsible: 'Maria Oliveira', status: 'Active' },
      { id: '3', name: 'Incentivada 50%', type: 'Energia Incentivada', responsible: 'Pedro Santos', status: 'Inactive' }
    ]);
  }
}
