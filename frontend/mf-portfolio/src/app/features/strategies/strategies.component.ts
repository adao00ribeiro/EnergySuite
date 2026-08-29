import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

interface Strategy {
  id: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-strategies',
  standalone: true,
  imports: [CommonModule, DragDropModule, MatCardModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  templateUrl: './strategies.component.html',
  styleUrls: ['./strategies.component.scss']
})
export class StrategiesComponent {
  private snackBar = inject(MatSnackBar);

  draft: Strategy[] = [];
  approved: Strategy[] = [];
  inactive: Strategy[] = [];

  drop(event: CdkDragDrop<Strategy[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
    }
  }

  onNewStrategy() {
    this.snackBar.open('Criação de estratégias requer o endpoint /api/v1/strategies no backend.', 'Fechar', {
      duration: 5000,
      panelClass: ['warn-snackbar']
    });
  }
}