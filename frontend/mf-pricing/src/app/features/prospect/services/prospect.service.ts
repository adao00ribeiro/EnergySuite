import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateStudyCommand {
  name: string;
  description?: string;
  model: string;
  startDate: string;
  horizonMonths: number;
}

export interface Study {
  id: string;
  name: string;
  description: string;
  model: string;
  startDate: string;
  horizonMonths: number;
  state: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProspectService {
  private http = inject(HttpClient);
  studies = signal<Study[]>([]);

  createStudy(command: CreateStudyCommand): Observable<any> {
    return this.http.post('/api/v1/prospect/studies', command);
  }

  // Mocking the data for the UI showcase
  loadStudies() {
    this.studies.set([
      {
        id: '1',
        name: 'PLD Setembro 2026',
        description: 'Estudo base PLD com premissas oficiais',
        model: 'NEWAVE + DECOMP',
        startDate: '2026-09-01T00:00:00Z',
        horizonMonths: 12,
        state: 'Completed',
        createdAt: new Date().toISOString()
      },
      {
        id: '2',
        name: 'Sensibilidade GSF - Outubro',
        description: 'Cenário pessimista de afluências',
        model: 'DECOMP',
        startDate: '2026-10-01T00:00:00Z',
        horizonMonths: 6,
        state: 'Running',
        createdAt: new Date().toISOString()
      },
      {
        id: '3',
        name: 'Previsão Semanal DESSEM',
        description: 'Despacho diário',
        model: 'DESSEM',
        startDate: '2026-08-25T00:00:00Z',
        horizonMonths: 1,
        state: 'Failed',
        createdAt: new Date().toISOString()
      }
    ]);
  }
}
