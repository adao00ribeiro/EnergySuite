import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContractService } from '../../services/contract.service';

@Component({
  selector: 'app-quick-action-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quick-action-cards.html',
  styleUrl: './quick-action-cards.css'
})
export class QuickActionCardsComponent {
  private contractService = inject(ContractService);

  actions = [
    { title: 'Nova Compra', description: 'Registrar contrato de energia', icon: 'plus-circle', color: 'indigo', actionType: 'Compra' },
    { title: 'Nova Venda', description: 'Registrar venda de energia', icon: 'arrow-up-right', color: 'violet', actionType: 'Venda' },
    { title: 'Liquidação', description: 'Processamento financeiro CCEE', icon: 'dollar-sign', color: 'amber', actionType: null },
  ];

  handleAction(action: any) {
    if (action.actionType) {
      const mockContract = {
        counterpartyName: action.actionType === 'Compra' ? 'Votener Energia' : 'Matrix Energia',
        type: action.actionType === 'Compra' ? 'Purchase' : 'Sale',
        submarket: 'SE_CO',
        volumeMwMed: Math.floor(Math.random() * 50) + 10,
        price: action.actionType === 'Compra' ? 120.5 : 135.0,
        startDate: new Date().toISOString(),
        endDate: new Date(new Date().setFullYear(new Date().getFullYear() + 1)).toISOString(),
      };
      
      this.contractService.createContract(mockContract).subscribe({
        next: () => console.log('Mock Contract Created Successfully!'),
        error: (err) => console.error('Error creating contract:', err)
      });
    }
  }
}
