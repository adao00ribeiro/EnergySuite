import { Component, OnInit, AfterViewInit, inject, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CompanyService, Company } from '../services/company.service';
import { NewCompanyDialogComponent } from '../components/new-company-dialog/new-company-dialog.component';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule, MatDialogModule, MatSnackBarModule],
  templateUrl: './company-list.html',
  styleUrl: './company-list.scss'
})
export class CompanyListComponent implements OnInit, AfterViewInit {
  private companyService = inject(CompanyService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns: string[] = ['cnpj', 'tradeName', 'category', 'location', 'cceeCode', 'actions'];
  dataSource = new MatTableDataSource<Company>([]);

  constructor() {
    effect(() => {
      const data = this.companyService.companies();
      this.dataSource.data = data;
    });
  }

  onNewCompany(): void {
    const dialogRef = this.dialog.open(NewCompanyDialogComponent, {
      width: '640px',
      maxWidth: '95vw',
      panelClass: 'glass-panel-dialog'
    });

    dialogRef.afterClosed().subscribe((saved) => {
      if (saved) {
        this.companyService.loadCompanies();
        this.snackBar.open('Registro empresarial criado com sucesso!', 'OK', { duration: 4000 });
      }
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  ngOnInit(): void {
    this.companyService.loadCompanies();
  }
}
