import { Component, OnInit, AfterViewInit, inject, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CompanyService, Company } from '../services/company.service';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './company-list.html',
  styleUrl: './company-list.scss'
})
export class CompanyListComponent implements OnInit, AfterViewInit {
  private companyService = inject(CompanyService);

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

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }

  ngOnInit(): void {
    this.companyService.loadCompanies();
  }
}
