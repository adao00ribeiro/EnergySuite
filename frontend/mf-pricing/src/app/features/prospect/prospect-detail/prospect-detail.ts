import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-prospect-detail',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatCardModule, MatListModule, MatIconModule],
  templateUrl: './prospect-detail.html',
  styleUrls: ['./prospect-detail.css']
})
export class ProspectDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);

  studyId: string | null = null;
  
  // Mock data for UI
  premissas = {
    cenarioGsf: 'Pessimista',
    crescimentoCarga: 2.5,
    niveisIniciais: [
      { submercado: 'SE/CO', nivel: 45.2 },
      { submercado: 'S', nivel: 80.1 },
      { submercado: 'NE', nivel: 60.5 },
      { submercado: 'N', nivel: 90.0 }
    ]
  };

  decks = [
    { mes: 'Janeiro/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Fevereiro/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Março/2027', status: 'Pending', icon: 'hourglass_empty' },
    { mes: 'Abril/2027', status: 'Pending', icon: 'hourglass_empty' },
  ];

  ngOnInit() {
    this.studyId = this.route.snapshot.paramMap.get('id');
  }
}
