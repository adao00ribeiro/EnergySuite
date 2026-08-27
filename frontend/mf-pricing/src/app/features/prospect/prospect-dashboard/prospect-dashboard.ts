import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ProspectService } from '../services/prospect.service';

import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-prospect-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './prospect-dashboard.html',
  styleUrls: ['./prospect-dashboard.css']
})
export class ProspectDashboardComponent implements OnInit {
  private prospectService = inject(ProspectService);
  
  studies = this.prospectService.studies;
  displayedColumns: string[] = ['name', 'model', 'startDate', 'horizon', 'state', 'actions'];

  ngOnInit() {
    this.prospectService.loadStudies();
  }

  getStateColor(state: string): string {
    switch(state) {
      case 'Completed': return 'primary';
      case 'Running': return 'accent';
      case 'Failed': return 'warn';
      default: return '';
    }
  }
}
