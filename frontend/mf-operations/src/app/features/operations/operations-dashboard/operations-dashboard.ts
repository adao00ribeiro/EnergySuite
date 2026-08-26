import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { QuickActionCardsComponent } from '../components/quick-action-cards/quick-action-cards';
import { ContractsTableComponent } from '../components/contracts-table/contracts-table';

@Component({
  selector: 'app-operations-dashboard',
  standalone: true,
  imports: [CommonModule, QuickActionCardsComponent, ContractsTableComponent],
  templateUrl: './operations-dashboard.html',
  styleUrl: './operations-dashboard.css'
})
export class OperationsDashboardComponent {
  currentDate = new Date();
}
