import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-settings-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    MatTabsModule, 
    MatCardModule, 
    MatIconModule, 
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    ReactiveFormsModule,
    MatSnackBarModule
  ],
  templateUrl: './settings-dashboard.component.html',
  styleUrls: ['./settings-dashboard.component.scss']
})
export class SettingsDashboardComponent {
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  settingsForm = this.fb.group({
    theme: ['dark'],
    language: ['pt-BR'],
    timezone: ['America/Sao_Paulo']
  });

  apiKeys: { id: string, name: string, token: string, createdAt: Date }[] = [
    { id: '1', name: 'Integração MLOps', token: 'ey...xxx', createdAt: new Date() }
  ];

  saveSettings() {
    // Integração mock para salvar backend
    this.snackBar.open('Preferências salvas com sucesso!', 'Fechar', { duration: 3000 });
  }

  generateApiKey() {
    this.apiKeys.push({
      id: Math.random().toString(36).substr(2, 9),
      name: 'Nova Chave M2M',
      token: 'ey...' + Math.random().toString(36),
      createdAt: new Date()
    });
    this.snackBar.open('Chave gerada com sucesso!', 'Fechar', { duration: 3000 });
  }

  revokeApiKey(id: string) {
    this.apiKeys = this.apiKeys.filter(k => k.id !== id);
    this.snackBar.open('Chave revogada!', 'Fechar', { duration: 3000, panelClass: 'warn-snackbar' });
  }
}
