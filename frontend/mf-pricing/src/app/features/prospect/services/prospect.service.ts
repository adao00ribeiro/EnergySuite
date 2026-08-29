import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

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

export interface StudyResult {
  month: string;
  pldSE: number;
  pldS: number;
  pldNE: number;
  pldN: number;
}

export interface StudyResultResponse {
  results: StudyResult[];
}

@Injectable({
  providedIn: 'root'
})
export class ProspectService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/prospect/studies`;

  studies = signal<Study[]>([]);
  isLoading = signal<boolean>(false);

  createStudy(command: CreateStudyCommand): Observable<any> {
    return this.http.post(this.apiUrl, command);
  }

  loadStudies() {
    this.isLoading.set(true);
    this.http.get<Study[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.studies.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.studies.set([]);
        this.isLoading.set(false);
      }
    });
  }

  executeStudy(studyId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${studyId}/execute`, {});
  }

  getStudyResults(studyId: string): Observable<StudyResultResponse> {
    return this.http.get<StudyResultResponse>(`${this.apiUrl}/${studyId}/results`);
  }

  cloneStudy(studyId: string): Observable<Study> {
    return this.http.post<Study>(`${this.apiUrl}/${studyId}/clone`, {});
  }
}
