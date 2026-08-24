import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ContractService } from '../../data-access/contract.service';

@Component({
  selector: 'app-contract-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule],
  template: `
    <div class="header">
      <h1>Contratos (ETRM)</h1>
      <button mat-raised-color="primary" mat-raised-button color="primary">
        <mat-icon>add</mat-icon>
        Novo Contrato
      </button>
    </div>

    <div class="table-container mat-elevation-z8">
      <table mat-table [dataSource]="contractService.contracts()">
        
        <!-- Counterparty Column -->
        <ng-container matColumnDef="counterpartyName">
          <th mat-header-cell *matHeaderCellDef> Contraparte </th>
          <td mat-cell *matCellDef="let element"> {{element.counterpartyName}} </td>
        </ng-container>

        <!-- Volume Column -->
        <ng-container matColumnDef="volumeMwMed">
          <th mat-header-cell *matHeaderCellDef> Volume (MWmed) </th>
          <td mat-cell *matCellDef="let element"> {{element.volumeMwMed}} </td>
        </ng-container>

        <!-- Price Column -->
        <ng-container matColumnDef="price">
          <th mat-header-cell *matHeaderCellDef> Preço </th>
          <td mat-cell *matCellDef="let element"> {{element.price | currency:'BRL'}} </td>
        </ng-container>

        <!-- Period Column -->
        <ng-container matColumnDef="period">
          <th mat-header-cell *matHeaderCellDef> Período </th>
          <td mat-cell *matCellDef="let element"> 
            {{element.startDate | date:'MM/yyyy'}} - {{element.endDate | date:'MM/yyyy'}} 
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <div *ngIf="contractService.isLoading()" class="loading-shade">
        Carregando contratos...
      </div>
    </div>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      
      h1 {
        margin: 0;
        font-size: 1.5rem;
        font-weight: 400;
      }
    }
    .table-container {
      position: relative;
      background: white;
      border-radius: 8px;
      overflow: hidden;
    }
    table {
      width: 100%;
    }
    .loading-shade {
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(255, 255, 255, 0.8);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10;
    }
  `]
})
export class ContractListComponent implements OnInit {
  public contractService = inject(ContractService);
  public displayedColumns: string[] = ['counterpartyName', 'volumeMwMed', 'price', 'period'];

  ngOnInit() {
    this.contractService.loadContracts();
  }
}
