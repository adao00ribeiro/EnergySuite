import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

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
  imports: [CommonModule, MatTableModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  displayedColumns: string[] = ['username', 'email', 'roles', 'lastAccess', 'status', 'actions'];
  users: UserAccess[] = [];

  ngOnInit() {
    // Mock data based on IAM Keycloak fetch
    this.users = [
      { id: 'u1', username: 'jsilva', email: 'joao.silva@energy.com', roles: ['Portfolio Manager', 'Trader'], lastAccess: new Date(), status: 'Active' },
      { id: 'u2', username: 'moliveira', email: 'maria.oliveira@energy.com', roles: ['Risk Analyst'], lastAccess: new Date(Date.now() - 86400000), status: 'Active' },
      { id: 'u3', username: 'rlima', email: 'roberto.lima@energy.com', roles: ['Viewer'], lastAccess: new Date(Date.now() - 1728000000), status: 'Suspended' }
    ];
  }

  editRoles(user: UserAccess) {
    alert(`Editar papéis para ${user.username} (Modal Keycloak) será aberto aqui.`);
  }

  viewLogs(user: UserAccess) {
    alert(`Visualizando logs de sessão para ${user.username}`);
  }

  getStatusColor(status: string): string {
    return status === 'Active' ? 'primary' : 'warn';
  }
}
