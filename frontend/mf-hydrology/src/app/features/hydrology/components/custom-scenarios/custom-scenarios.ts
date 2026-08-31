import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSliderModule } from '@angular/material/slider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-custom-scenarios',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatSliderModule,
    MatSnackBarModule
  ],
  templateUrl: './custom-scenarios.html',
  styleUrl: './custom-scenarios.css'
})
export class CustomScenariosComponent {
  uploadForm: FormGroup;
  blendForm: FormGroup;
  selectedFile: File | null = null;
  isUploading = false;
  isBlending = false;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {
    this.uploadForm = this.fb.group({
      scenarioName: ['', Validators.required],
      horizonDays: [14, [Validators.required, Validators.min(1)]]
    });

    this.blendForm = this.fb.group({
      scenarioName: ['', Validators.required],
      gefsWeight: [50, Validators.required],
      etaWeight: [30, Validators.required],
      ecmwfWeight: [20, Validators.required]
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onUpload() {
    if (this.uploadForm.valid && this.selectedFile) {
      this.isUploading = true;
      const formData = new FormData();
      formData.append('file', this.selectedFile);
      formData.append('name', this.uploadForm.get('scenarioName')?.value);
      formData.append('referenceDate', new Date().toISOString());
      formData.append('horizonDays', this.uploadForm.get('horizonDays')?.value);

      this.http.post(`${environment.apiUrl}/pluvia/custom-maps/upload`, formData).subscribe({
        next: (res) => {
          this.isUploading = false;
          this.snackBar.open('Upload de mapa customizado concluído com sucesso!', 'OK', { duration: 4000 });
        },
        error: (err) => {
          this.isUploading = false;
          this.snackBar.open('Erro ao enviar mapa customizado. Tente novamente.', 'Fechar', {
            duration: 5000,
            panelClass: 'warn-snackbar'
          });
        }
      });
    }
  }

  onBlend() {
    if (this.blendForm.valid) {
      this.isBlending = true;
      const totalWeight = this.blendForm.value.gefsWeight + this.blendForm.value.etaWeight + this.blendForm.value.ecmwfWeight;
      
      if (totalWeight !== 100) {
        this.snackBar.open('A soma dos pesos (GEFS + ETA + ECMWF) deve ser exatamente 100%.', 'Atenção', {
          duration: 4000,
          panelClass: 'warn-snackbar'
        });
        this.isBlending = false;
        return;
      }

      const payload = {
        name: this.blendForm.value.scenarioName,
        referenceDate: new Date().toISOString(),
        horizonDays: 14,
        blendConfig: JSON.stringify({
          GEFS: this.blendForm.value.gefsWeight / 100,
          ETA: this.blendForm.value.etaWeight / 100,
          ECMWF: this.blendForm.value.ecmwfWeight / 100
        })
      };

      this.http.post(`${environment.apiUrl}/pluvia/custom-maps/blend`, payload).subscribe({
        next: (res) => {
          this.isBlending = false;
          this.snackBar.open('Cenário combinado gerado com sucesso!', 'OK', { duration: 4000 });
        },
        error: (err) => {
          this.isBlending = false;
          this.snackBar.open('Erro ao combinar cenários hidrológicos.', 'Fechar', {
            duration: 5000,
            panelClass: 'warn-snackbar'
          });
        }
      });
    }
  }
}

