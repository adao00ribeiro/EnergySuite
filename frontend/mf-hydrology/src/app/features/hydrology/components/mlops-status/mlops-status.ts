import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface MlopsRun {
  modelName: string;
  status: 'running' | 'success' | 'failed';
  accuracy: string;
  lastRun: string;
}

@Component({
  selector: 'app-mlops-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mlops-status.html',
  styleUrl: './mlops-status.css'
})
export class MlopsStatusComponent implements OnInit {
  runs: MlopsRun[] = [
    { modelName: 'NEWAVE - Chuva-Vazão', status: 'success', accuracy: 'MSE: 0.042', lastRun: 'Hoje, 04:30 AM' },
    { modelName: 'DECOMP - Otimização', status: 'success', accuracy: 'RMSE: 0.11', lastRun: 'Hoje, 05:15 AM' },
    { modelName: 'Rede Neural - ENA Mensal', status: 'running', accuracy: 'Calculando...', lastRun: 'Em andamento' }
  ];
  
  ngOnInit(): void {}
}
