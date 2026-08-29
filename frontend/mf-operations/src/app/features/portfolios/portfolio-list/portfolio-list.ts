import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

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
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './portfolio-list.html',
  styleUrl: './portfolio-list.scss'
})
export class PortfolioListComponent implements OnInit {
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);

  displayedColumns: string[] = ['name', 'type', 'responsible', 'status', 'actions'];
  dataSource = signal<Portfolio[]>([]);

  ngOnInit(): void {
    this.loadPortfolios();
  }

  loadPortfolios() {
    this.http.get<Portfolio[]>(`${environment.apiUrl}/portfolios`).subscribe({
      next: (data) => {
        this.dataSource.set(Array.isArray(data) ? data : []);
      },
      error: (err) => {
        this.dataSource.set([]);
        console.error('Failed to load portfolios:', err);
        this.snackBar.open('Não foi possível carregar os portfólios.', 'Fechar', {
          duration: 5000,
          panelClass: ['warn-snackbar']
        });
      }
    });
  }
}