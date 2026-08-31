import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-indicator-cards',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="cards-grid">
      <mat-card class="indicator-card premium-card">
        <mat-card-header>
          <div mat-card-avatar class="icon-avatar bg-blue">
            <mat-icon>shopping_cart</mat-icon>
          </div>
          <mat-card-title>Volume Comprado</mat-card-title>
          <mat-card-subtitle>MWmédio</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <h2 class="value text-blue">{{ purchased | number:'1.1-2' }}</h2>
        </mat-card-content>
      </mat-card>

      <mat-card class="indicator-card premium-card">
        <mat-card-header>
          <div mat-card-avatar class="icon-avatar bg-orange">
            <mat-icon>sell</mat-icon>
          </div>
          <mat-card-title>Volume Vendido</mat-card-title>
          <mat-card-subtitle>MWmédio</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <h2 class="value text-orange">{{ sold | number:'1.1-2' }}</h2>
        </mat-card-content>
      </mat-card>

      <mat-card class="indicator-card premium-card">
        <mat-card-header>
          <div mat-card-avatar class="icon-avatar bg-purple">
            <mat-icon>balance</mat-icon>
          </div>
          <mat-card-title>Posição Líquida</mat-card-title>
          <mat-card-subtitle>MWmédio</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <h2 class="value text-purple">{{ net | number:'1.1-2' }}</h2>
        </mat-card-content>
      </mat-card>

      <mat-card class="indicator-card premium-card">
        <mat-card-header>
          <div mat-card-avatar class="icon-avatar bg-green">
            <mat-icon>attach_money</mat-icon>
          </div>
          <mat-card-title>Resultado Estimado</mat-card-title>
          <mat-card-subtitle>R$ (Milhares)</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <h2 class="value text-green">{{ estimatedResult | currency:'BRL' }}</h2>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }
    .premium-card {
      border-radius: 12px;
      box-shadow: var(--shadow-sm);
      transition: transform 200ms ease, box-shadow 200ms ease;
      background: var(--color-card);
      color: var(--color-foreground);
    }
    .premium-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-lg);
    }
    .icon-avatar {
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      color: var(--color-on-accent);
    }
    .bg-blue { background-color: var(--color-info); }
    .bg-orange { background-color: var(--color-warning); }
    .bg-purple { background-color: var(--color-operations); }
    .bg-green { background-color: var(--color-success); }
    
    .text-blue { color: var(--color-info); }
    .text-orange { color: var(--color-warning); }
    .text-purple { color: var(--color-operations); }
    .text-green { color: var(--color-success); }

    .value {
      font-size: 2rem;
      font-weight: 700;
      margin-top: 1rem;
      margin-bottom: 0;
    }
  `]
})
export class IndicatorCardsComponent {
  @Input() purchased = 0;
  @Input() sold = 0;
  @Input() net = 0;
  @Input() estimatedResult = 0;
}
