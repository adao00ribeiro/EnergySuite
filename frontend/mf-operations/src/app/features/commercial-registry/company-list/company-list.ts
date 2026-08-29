import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
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
export class CompanyListComponent implements OnInit {
  private companyService = inject(CompanyService);

  displayedColumns: string[] = ['cnpj', 'tradeName', 'category', 'location', 'cceeCode', 'actions'];
  dataSource = this.companyService.companies;

  ngOnInit(): void {
    this.companyService.loadCompanies();
  }
}
