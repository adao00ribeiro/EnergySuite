import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ProspectService } from '../services/prospect.service';
import { Router, RouterModule } from '@angular/router';
import { NewStudyDialogComponent } from '../components/new-study-dialog/new-study-dialog.component';

@Component({
  selector: 'app-prospect-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule, 
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './prospect-dashboard.html',
  styleUrls: ['./prospect-dashboard.css']
})
export class ProspectDashboardComponent implements OnInit {
  private prospectService = inject(ProspectService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  studies: any[] = [];
  displayedColumns: string[] = ['name', 'model', 'startDate', 'horizon', 'state', 'actions'];

  ngOnInit() {
    this.loadStudies();
  }

  loadStudies() {
    // For now, load mock data but could be tied to service if available
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
    const dialogRef = this.dialog.open(NewStudyDialogComponent, {
      width: '500px',
      panelClass: 'glass-panel',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Append missing fields expected by the backend DTO if necessary
        const payload = {
          ...result,
          startDate: result.startDate.toISOString()
        };

        this.prospectService.createStudy(payload).subscribe({
          next: () => {
            this.snackBar.open('Estudo criado com sucesso!', 'Fechar', { duration: 3000 });
            this.loadStudies(); // Reload table data
          },
          error: (err) => {
            console.error('Error creating study:', err);
            this.snackBar.open('Falha ao criar o estudo. Tente novamente.', 'Fechar', { 
              duration: 5000, 
              panelClass: ['warn-snackbar'] 
            });
            // As a fallback for UI demonstration since backend might not be up:
            const newId = this.studies.length + 1;
            this.studies = [
              {
                id: newId,
                name: payload.name,
                author: 'Usuário Atual',
                date: payload.startDate.split('T')[0],
                status: 'Running'
              },
              ...this.studies
            ];
          }
        });
      }
    });
  }
}
