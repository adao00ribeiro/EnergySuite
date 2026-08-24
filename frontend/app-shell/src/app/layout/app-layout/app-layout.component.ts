import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatListModule,
    MatButtonModule
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #sidenav mode="side" opened class="sidenav">
        <div class="logo-container">
          <h2>Norus ETRM</h2>
        </div>
        <mat-nav-list>
          <a mat-list-item routerLink="/contracts" routerLinkActive="active-link">
            <mat-icon matListItemIcon>description</mat-icon>
            <div matListItemTitle>Contracts</div>
          </a>
          <a mat-list-item routerLink="/" routerLinkActive="active-link" [routerLinkActiveOptions]="{exact: true}">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <div matListItemTitle>Dashboard</div>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar color="primary" class="toolbar">
          <button mat-icon-button (click)="sidenav.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          <span>EnergySuite Workspace</span>
          <span class="spacer"></span>
          <button mat-icon-button>
            <mat-icon>account_circle</mat-icon>
          </button>
        </mat-toolbar>
        
        <main class="content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100vh;
    }
    .sidenav {
      width: 250px;
      background-color: #1e1e2d;
      color: white;
    }
    .logo-container {
      height: 64px;
      display: flex;
      align-items: center;
      padding: 0 16px;
      background-color: #1a1a27;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
      
      h2 {
        margin: 0;
        font-weight: 500;
        font-size: 1.2rem;
        color: #ffffff;
      }
    }
    .mat-mdc-nav-list {
      padding-top: 0;
    }
    .mat-mdc-list-item {
      color: rgba(255, 255, 255, 0.7);
      
      &.active-link {
        color: #ffffff;
        background-color: rgba(255, 255, 255, 0.1);
        border-left: 4px solid #69b3ff;
      }
      
      .mat-icon {
        color: inherit;
      }
    }
    .toolbar {
      background-color: #ffffff;
      color: #333333;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .spacer {
      flex: 1 1 auto;
    }
    .content {
      padding: 24px;
      height: calc(100vh - 64px - 48px);
      overflow: auto;
      background-color: #f5f8fa;
      color: #333333;
    }
  `]
})
export class AppLayoutComponent {}
