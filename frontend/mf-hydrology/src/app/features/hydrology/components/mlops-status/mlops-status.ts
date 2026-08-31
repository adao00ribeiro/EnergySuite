import { Component, OnInit, signal, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

interface MlopsRun {
  id: string;
  modelName: string;
  status: 'running' | 'completed' | 'failed' | 'pending';
  accuracy: string;
  startedAt: string;
}

@Component({
  selector: 'app-mlops-status',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './mlops-status.html',
  styleUrl: './mlops-status.css'
})
export class MlopsStatusComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private intervalId: any;
  
  runs = signal<MlopsRun[]>([]);
  
  ngOnInit(): void {
    this.fetchExecutions();
    // Poll every 10 seconds for real-time status updates
    this.intervalId = setInterval(() => this.fetchExecutions(), 10000);
  }
  
  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  fetchExecutions() {
    this.http.get<MlopsRun[]>(`${environment.apiUrl}/pluvia/executions`).subscribe({
      next: (data) => {
        this.runs.set(Array.isArray(data) ? data : []);
      },
      error: (err) => {
        this.runs.set([]);
        console.error('Failed to load mlops executions', err);
      }
    });
  }
  
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
  }
}
