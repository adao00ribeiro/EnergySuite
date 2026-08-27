import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export interface SimulationData {
  opportunityId: string;
  name: string;
}

@Component({
  selector: 'app-simulation-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './simulation-dialog.component.html',
  styleUrls: ['./simulation-dialog.component.scss']
})
export class SimulationDialogComponent implements OnInit {
  isLoading = true;
  result: any = null;

  constructor(
    public dialogRef: MatDialogRef<SimulationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SimulationData
  ) {}

  ngOnInit() {
    this.simulateOperation();
  }

  simulateOperation() {
    // Mock the HTTP call to SimulateOperationCommand
    setTimeout(() => {
      this.result = {
        previousVolumeMwm: 30.5,
        newVolumeMwm: 46.0,
        volumeDelta: 15.5,
        previousEstimatedResult: 450000.00,
        newEstimatedResult: 438000.00,
        financialDelta: -12000.00,
        copilotAnalysis: {
          summaryText: "Esta é uma operação de hedge/cobertura. A aquisição de 15.5 MWm terá um custo (redução no resultado estimado) de R$ 12,000.00. O déficit no submercado será reduzido significativamente.",
          recommendation: "Approve"
        }
      };
      this.isLoading = false;
    }, 1500);
  }

  close(approved: boolean = false) {
    this.dialogRef.close(approved);
  }
}
