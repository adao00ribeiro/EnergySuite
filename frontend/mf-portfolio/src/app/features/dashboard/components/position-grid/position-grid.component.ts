import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

@Component({
  selector: 'app-position-grid',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule],
  template: `
    <div class="table-container mat-elevation-z2">
      <table mat-table [dataSource]="dataSource" matSort>
        <ng-container matColumnDef="month">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Mês </th>
          <td mat-cell *matCellDef="let row"> {{row.month}} </td>
        </ng-container>

        <ng-container matColumnDef="submarket">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Submercado </th>
          <td mat-cell *matCellDef="let row"> {{row.submarket}} </td>
        </ng-container>

        <ng-container matColumnDef="energySource">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Fonte </th>
          <td mat-cell *matCellDef="let row"> {{row.energySource}} </td>
        </ng-container>

        <ng-container matColumnDef="purchased">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Compra (MWm) </th>
          <td mat-cell *matCellDef="let row" class="text-blue"> {{row.purchased | number:'1.1-2'}} </td>
        </ng-container>

        <ng-container matColumnDef="sold">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Venda (MWm) </th>
          <td mat-cell *matCellDef="let row" class="text-orange"> {{row.sold | number:'1.1-2'}} </td>
        </ng-container>

        <ng-container matColumnDef="netGap">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Gap/Sobra </th>
          <td mat-cell *matCellDef="let row" [ngClass]="{'text-red': row.netGap < 0, 'text-green': row.netGap >= 0}">
            <strong>{{row.netGap | number:'1.1-2'}}</strong>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>

      <mat-paginator [pageSizeOptions]="[10, 25, 100]" aria-label="Selecione a página de gaps"></mat-paginator>
    </div>
  `,
  styles: [`
    .table-container {
      border-radius: 8px;
      overflow: hidden;
      margin-top: 16px;
      background: var(--color-card);
    }
    table {
      width: 100%;
    }
    .text-blue { color: var(--color-info); }
    .text-orange { color: var(--color-warning); }
    .text-red { color: var(--color-destructive); }
    .text-green { color: var(--color-success); }
  `]
})
export class PositionGridComponent implements OnChanges {
  @Input() data: any[] = [];
  
  displayedColumns: string[] = ['month', 'submarket', 'energySource', 'purchased', 'sold', 'netGap'];
  dataSource: MatTableDataSource<any> = new MatTableDataSource();

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.dataSource = new MatTableDataSource(this.data);
      if (this.paginator) {
        this.dataSource.paginator = this.paginator;
      }
      if (this.sort) {
        this.dataSource.sort = this.sort;
      }
    }
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }
}
