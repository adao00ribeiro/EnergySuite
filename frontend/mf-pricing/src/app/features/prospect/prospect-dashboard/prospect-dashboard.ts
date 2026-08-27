import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ProspectService } from '../services/prospect.service';

import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-prospect-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './prospect-dashboard.html',
  styleUrls: ['./prospect-dashboard.css']
})
export class ProspectDashboardComponent implements OnInit {
  private prospectService = inject(ProspectService);
  private router = inject(Router);

  studies: any[] = [];
  displayedColumns: string[] = ['name', 'model', 'startDate', 'horizon', 'state', 'actions'];

  ngOnInit() {
    this.loadStudies();
  }

  loadStudies() {
    this.studies = [
      { id: 1, name: 'Estudo PLD 2026', author: 'João Silva', date: '2026-08-20', status: 'Completed' },
      { id: 2, name: 'Cenário Base (PDE)', author: 'Maria Oliveira', date: '2026-08-21', status: 'Running' },
      { id: 3, name: 'Estudo PLD 2027', author: 'João Silva', date: '2026-08-22', status: 'Pending' }
    ];
  }

  viewDetails(id: number) {
    this.router.navigate(['/prospect', id]);
  }

  cloneStudy(study: any) {
    // Fake API call to Clone endpoint
    const newId = this.studies.length + 1;
    this.studies = [
      {
        id: newId,
        name: study.name + ' (Cloned)',
        author: 'Usuário Atual',
        date: new Date().toISOString().split('T')[0],
        status: 'Pending'
      },
      ...this.studies
    ];
  }

  getStateColor(state: string): string {
    switch (state) {
      case 'Completed': return 'primary';
      case 'Running': return 'accent';
      case 'Failed': return 'warn';
      default: return '';
    }
  }

  onNewStudy() {
    alert('Abertura do formulário de "Novo Estudo" em desenvolvimento na Sprint atual!');
  }
}
