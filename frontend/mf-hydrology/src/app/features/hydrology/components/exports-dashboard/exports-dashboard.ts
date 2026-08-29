import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';

interface ExportFile {
  fileName: string;
  fileType: string;
  sizeBytes: number;
  downloadUrl: string;
}

interface Execution {
  id: string;
  modelName: string;
  status: string;
  accuracy?: number;
  startedAt?: string;
  completedAt?: string;
}

@Component({
  selector: 'app-exports-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './exports-dashboard.html',
  styleUrl: './exports-dashboard.css'
})
export class ExportsDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  
  exports = signal<ExportFile[]>([]);
  executions = signal<Execution[]>([]);
  selectedExecutionId = signal<string>('');
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  displayedColumns: string[] = ['fileType', 'fileName', 'size', 'action'];

  ngOnInit(): void {
    this.loadExecutions();
  }

  loadExecutions() {
    this.isLoading.set(true);
    this.error.set(null);
    this.http.get<Execution[]>('/api/v1/pluvia/executions').subscribe({
      next: (data) => {
        this.executions.set(data);
        const completed = data.find((e) => e.status === 'completed');
        const selected = completed ?? data[0];
        this.selectedExecutionId.set(selected ? selected.id : '');
        this.isLoading.set(false);
        if (selected) {
          this.loadExports(selected.id);
        }
      },
      error: (err) => {
        console.error('Failed to load executions', err);
        this.error.set('Falha ao carregar execuções.');
        this.isLoading.set(false);
      }
    });
  }

  loadExports(executionId: string) {
    if (!executionId) {
      this.exports.set([]);
      return;
    }
    this.isLoading.set(true);
    this.error.set(null);
    this.http.get<ExportFile[]>(`/api/v1/pluvia/exports/${executionId}`).subscribe({
      next: (data) => {
        this.exports.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load exports', err);
        this.error.set('Falha ao carregar dados do Lakehouse.');
        this.exports.set([]);
        this.isLoading.set(false);
      }
    });
  }

  onExecutionChange(id: string) {
    this.selectedExecutionId.set(id);
    this.loadExports(id);
  }

  formatBytes(bytes: number, decimals = 2) {
    if (!+bytes) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
  }
}
