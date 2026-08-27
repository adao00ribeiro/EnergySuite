import { Component, OnInit, inject, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatChipsModule } from '@angular/material/chips';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SelectionModel } from '@angular/cdk/collections';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CceeIntegrationService, CceeComparisonDto } from '../services/ccee-integration.service';
import { effect } from '@angular/core';

@Component({
  selector: 'app-ccee-comparison-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatChipsModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './ccee-comparison-list.html',
  styleUrls: ['./ccee-comparison-list.scss']
})
export class CceeComparisonListComponent implements OnInit {
  cceeService = inject(CceeIntegrationService);
  
  displayedColumns: string[] = ['select', 'period', 'counterpartyCceeCode', 'backOpsVolume', 'cceeVolume', 'difference', 'status'];
  dataSource = new MatTableDataSource<CceeComparisonDto>([]);
  selection = new SelectionModel<CceeComparisonDto>(true, []);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    effect(() => {
      this.dataSource.data = this.cceeService.comparisons();
    });
  }

  ngOnInit(): void {
    this.cceeService.loadComparisons();
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }
    this.selection.select(...this.dataSource.data);
  }

  checkboxLabel(row?: CceeComparisonDto): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${row.id}`;
  }

  generateAdjustments() {
    const selectedIds = this.selection.selected.map(s => s.id);
    if (selectedIds.length === 0) return;

    this.cceeService.generateAdjustments(selectedIds).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `ccee_adjustments_${new Date().getTime()}.xml`;
      a.click();
      window.URL.revokeObjectURL(url);
      
      // Reload comparisons to reflect updated status
      this.cceeService.loadComparisons();
      this.selection.clear();
    });
  }
}
