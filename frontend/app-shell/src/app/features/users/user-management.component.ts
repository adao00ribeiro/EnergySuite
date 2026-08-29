import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { EditRolesDialogComponent } from './edit-roles-dialog.component';

export interface UserAccess {
  id: string;
  username: string;
  email: string;
  roles: string[];
  lastAccess: Date;
  status: 'Active' | 'Suspended';
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  displayedColumns: string[] = ['username', 'email', 'roles', 'lastAccess', 'status', 'actions'];
  users: UserAccess[] = [];

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.http.get<UserAccess[]>(`${environment.apiUrl}/users`).subscribe({
      next: (data) => {
        this.users = Array.isArray(data) ? data : [];
      },
      error: (err) => {
        this.users = [];
        console.error('Failed to load users from IAM:', err);
        this.snackBar.open('Não foi possível carregar os usuários do Keycloak.', 'Fechar', {
          duration: 5000,
          panelClass: 'warn-snackbar'
        });
      }
    });
  }

  editRoles(user: UserAccess) {
    const dialogRef = this.dialog.open(EditRolesDialogComponent, {
      width: '420px',
      panelClass: 'glass-panel',
      disableClose: true,
      data: {
        userId: user.id,
        username: user.username,
        roles: user.roles
      }
    });

    dialogRef.afterClosed().subscribe(roles => {
      if (roles) {
        this.http.put(`${environment.apiUrl}/users/${user.id}/roles`, roles).subscribe({
          next: () => {
            user.roles = roles;
            this.snackBar.open('Papéis atualizados com sucesso.', 'Fechar', { duration: 3000 });
          },
          error: (err) => {
            console.error('Failed to update roles:', err);
            this.snackBar.open('Falha ao atualizar os papéis no Keycloak.', 'Fechar', {
              duration: 5000,
              panelClass: 'warn-snackbar'
            });
          }
        });
      }
    });
  }

  viewLogs(user: UserAccess) {
    this.snackBar.open(`Logs de sessão de ${user.username} não disponíveis no backend.`, 'Fechar', { duration: 4000 });
  }

  getStatusColor(status: string): string {
    return status === 'Active' ? 'primary' : 'warn';
  }
}