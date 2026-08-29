import { Component, OnInit, inject } from '@angular/core';
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
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface AppSettingsResponse {
  theme: string;
  language: string;
  timezone: string;
}

interface ApiKey {
  id: string;
  name: string;
  token: string;
  createdAt: Date;
}

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
export class SettingsDashboardComponent implements OnInit {
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  private http = inject(HttpClient);

  settingsForm = this.fb.group({
    theme: ['dark'],
    language: ['pt-BR'],
    timezone: ['America/Sao_Paulo']
  });

  apiKeys: ApiKey[] = [];

  ngOnInit() {
    this.loadSettings();
  }

  loadSettings() {
    this.http.get<AppSettingsResponse>(`${environment.apiUrl}/settings`).subscribe({
      next: (settings) => {
        if (settings?.theme) this.settingsForm.patchValue({ theme: settings.theme });
        if (settings?.language) this.settingsForm.patchValue({ language: settings.language });
        if (settings?.timezone) this.settingsForm.patchValue({ timezone: settings.timezone });
      },
      error: () => {
        this.snackBar.open('Não foi possível carregar as preferências.', 'Fechar', {
          duration: 5000,
          panelClass: 'warn-snackbar'
        });
      }
    });
  }

  saveSettings() {
    this.http.put(`${environment.apiUrl}/settings`, this.settingsForm.value).subscribe({
      next: () => {
        this.snackBar.open('Preferências salvas com sucesso!', 'Fechar', { duration: 3000 });
      },
      error: (err) => {
        console.error('Failed to save settings:', err);
        this.snackBar.open('Falha ao salvar as preferências.', 'Fechar', {
          duration: 5000,
          panelClass: 'warn-snackbar'
        });
      }
    });
  }

  generateApiKey() {
    this.http.post<ApiKey>(`${environment.apiUrl}/settings/m2m-tokens`, {}).subscribe({
      next: (key) => {
        this.apiKeys = [key, ...this.apiKeys];
        this.snackBar.open('Chave gerada com sucesso!', 'Fechar', { duration: 3000 });
      },
      error: (err) => {
        console.error('Failed to generate API key:', err);
        this.snackBar.open('Falha ao gerar a chave M2M.', 'Fechar', {
          duration: 5000,
          panelClass: 'warn-snackbar'
        });
      }
    });
  }

  revokeApiKey(id: string) {
    this.apiKeys = this.apiKeys.filter(k => k.id !== id);
    this.snackBar.open('Chave revogada.', 'Fechar', { duration: 3000, panelClass: 'warn-snackbar' });
  }
}