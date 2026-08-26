import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

interface ExportFile {
  fileName: string;
  fileType: string;
  sizeBytes: number;
  downloadUrl: string;
}

@Component({
  selector: 'app-exports-dashboard',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './exports-dashboard.html',
  styleUrl: './exports-dashboard.css'
})
export class ExportsDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  
  exports = signal<ExportFile[]>([]);
  displayedColumns: string[] = ['fileType', 'fileName', 'size', 'action'];
  
  // Using a mock GUID for the latest execution in this mock UI
  private mockExecutionId = 'd290f1ee-6c54-4b01-90e6-d701748f0851';

  ngOnInit(): void {
    this.loadExports();
  }

  loadExports() {
    this.http.get<ExportFile[]>(`/api/v1/pluvia/exports/${this.mockExecutionId}`).subscribe({
      next: (data) => this.exports.set(data),
      error: (err) => console.error('Failed to load exports', err)
    });
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
