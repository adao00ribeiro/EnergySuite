import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipListbox, MatChip } from '@angular/material/chips';

interface Strategy {
  id: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-strategies',
  standalone: true,
  imports: [CommonModule, DragDropModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './strategies.component.html',
  styleUrls: ['./strategies.component.scss']
})
export class StrategiesComponent {
  draft: Strategy[] = [
    { id: '1', name: 'Hedge de Inverno', description: 'Proteção contra preços em época de seca' }
  ];

  approved: Strategy[] = [
    { id: '2', name: 'Arbitragem Sul x SE', description: 'Compra no Sul e venda no SE aproveitando spread' },
    { id: '3', name: 'Venda Excedente Eólica', description: 'Desovar excedentes do NE' }
  ];

  inactive: Strategy[] = [
    { id: '4', name: 'Especulação Curto Prazo', description: 'Day trade no PLD' }
  ];

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
}
