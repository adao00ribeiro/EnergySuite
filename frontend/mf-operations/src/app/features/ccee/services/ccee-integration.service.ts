import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CceeComparisonDto {
  id: string;
  operationId?: string;
  counterpartyCceeCode: string;
  period: string;
  backOpsVolume: number;
  cceeVolume: number;
  difference: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class CceeIntegrationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/ccee-integration`;

  // Signals for state management
  comparisons = signal<CceeComparisonDto[]>([]);
  isLoading = signal<boolean>(false);

  exportCceal(start: string, end: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export-cceal?start=${start}&end=${end}`, { responseType: 'blob' });
  }

  uploadCliqCcee(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/upload-cliqccee`, formData);
  }

  loadComparisons(): void {
    this.isLoading.set(true);
    this.http.get<CceeComparisonDto[]>(`${this.apiUrl}/comparisons`).subscribe({
      next: (data) => {
        this.comparisons.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.comparisons.set([]);
        this.isLoading.set(false);
      }
    });
  }

  generateAdjustments(comparisonIds: string[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/generate-adjustments`, { comparisonIds }, { responseType: 'blob' });
  }
}
