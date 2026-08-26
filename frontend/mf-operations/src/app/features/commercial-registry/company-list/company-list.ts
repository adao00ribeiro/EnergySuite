import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface Company {
  id: string;
  cnpj: string;
  corporateName: string;
  tradeName: string;
  category: string;
  city: string;
  state: string;
  cceeCode: string;
}

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatSortModule, MatIconModule, MatButtonModule],
  templateUrl: './company-list.html',
  styleUrl: './company-list.scss'
})
export class CompanyListComponent implements OnInit {
  displayedColumns: string[] = ['cnpj', 'tradeName', 'category', 'location', 'cceeCode', 'actions'];
  dataSource = signal<Company[]>([]);

  ngOnInit(): void {
    // Mock data for now until API is connected
    this.dataSource.set([
      { id: '1', cnpj: '12.345.678/0001-90', corporateName: 'Matrix Energia S/A', tradeName: 'Matrix', category: 'Contraparte', city: 'São Paulo', state: 'SP', cceeCode: 'MAT01' },
      { id: '2', cnpj: '98.765.432/0001-10', corporateName: 'Votener SA', tradeName: 'Votener', category: 'Contraparte', city: 'Rio de Janeiro', state: 'RJ', cceeCode: 'VOT02' }
    ]);
  }
}
