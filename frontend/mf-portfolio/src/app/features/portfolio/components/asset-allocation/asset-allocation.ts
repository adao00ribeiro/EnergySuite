import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-asset-allocation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './asset-allocation.html',
  styleUrl: './asset-allocation.scss'
})
export class AssetAllocationComponent {
  allocations = [
    { title: 'Geração Eólica', value: '120 MWm', percentage: 45, color: 'blue' },
    { title: 'Geração Solar', value: '80 MWm', percentage: 30, color: 'slate' },
    { title: 'Contratos Bilaterais', value: '65 MWm', percentage: 25, color: 'rose' }
  ];
}
