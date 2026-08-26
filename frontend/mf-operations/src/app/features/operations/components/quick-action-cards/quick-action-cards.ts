import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-quick-action-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quick-action-cards.html',
  styleUrl: './quick-action-cards.css'
})
export class QuickActionCardsComponent {
  actions = [
    { title: 'Nova Compra', description: 'Registrar contrato de energia', icon: 'plus-circle', color: 'indigo' },
    { title: 'Nova Venda', description: 'Registrar venda de energia', icon: 'arrow-up-right', color: 'violet' },
    { title: 'Liquidação', description: 'Processamento financeiro CCEE', icon: 'dollar-sign', color: 'amber' },
  ];
}
