import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ProspectService, Study } from '../services/prospect.service';
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
  styleUrl: './prospect-dashboard.css'
})
export class ProspectDashboardComponent implements OnInit {
  private prospectService = inject(ProspectService);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  studies = this.prospectService.studies;
  isLoading = this.prospectService.isLoading;
  displayedColumns: string[] = ['name', 'model', 'startDate', 'horizon', 'state', 'actions'];

  ngOnInit() {
    this.loadStudies();
  }

  loadStudies() {
    this.prospectService.loadStudies();
  }

  viewDetails(id: string) {
    this.router.navigate(['/prospect', id]);
  }

  cloneStudy(study: Study) {
    this.prospectService.cloneStudy(study.id).subscribe({
      next: () => {
        this.snackBar.open('Estudo clonado com sucesso!', 'Fechar', { duration: 3000 });
        this.loadStudies();
      },
      error: (err) => {
        console.error('Erro ao clonar estudo', err);
        this.snackBar.open('Falha ao clonar o estudo. Tente novamente.', 'Fechar', {
          duration: 5000,
          panelClass: ['warn-snackbar']
        });
      }
    });
  }

  getStateColor(state: string): string {
    switch (state) {
      case 'Completed': return 'primary';
      case 'Running':
      case 'Queued': return 'accent';
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
        const payload = {
          ...result,
          startDate: result.startDate.toISOString()
        };

        this.prospectService.createStudy(payload).subscribe({
          next: () => {
            this.snackBar.open('Estudo criado com sucesso!', 'Fechar', { duration: 3000 });
            this.loadStudies();
          },
          error: (err) => {
            console.error('Error creating study:', err);
            this.snackBar.open('Falha ao criar o estudo. Tente novamente.', 'Fechar', {
              duration: 5000,
              panelClass: ['warn-snackbar']
            });
          }
        });
      }
    });
  }
}