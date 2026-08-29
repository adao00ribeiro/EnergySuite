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

  loadStudies() {
    this.http.get<Study[]>('/api/v1/prospect/studies').subscribe({
      next: (data) => this.studies.set(data),
      error: (err) => console.error('Erro ao buscar estudos', err)
    });
  }
}
