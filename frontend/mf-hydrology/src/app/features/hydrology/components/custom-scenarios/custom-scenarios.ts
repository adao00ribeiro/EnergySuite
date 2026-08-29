import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSliderModule } from '@angular/material/slider';
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
    MatSliderModule
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

  constructor(private fb: FormBuilder, private http: HttpClient) {
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
          alert('Upload concluído com sucesso!');
        },
        error: (err) => {
          this.isUploading = false;
          alert('Erro ao enviar mapa customizado.');
        }
      });
    }
  }

  onBlend() {
    if (this.blendForm.valid) {
      this.isBlending = true;
      const totalWeight = this.blendForm.value.gefsWeight + this.blendForm.value.etaWeight + this.blendForm.value.ecmwfWeight;
      
      if (totalWeight !== 100) {
        alert('A soma dos pesos deve ser exatamente 100%.');
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
          alert('Cenário combinado criado com sucesso!');
        },
        error: (err) => {
          this.isBlending = false;
          alert('Erro ao combinar cenários.');
        }
      });
    }
  }
}
