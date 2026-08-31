import { Component, input } from '@angular/core';
import { NgClass, NgFor, NgIf } from '@angular/common';

export type SkeletonVariant = 'card' | 'table' | 'text' | 'chart';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [NgClass, NgFor, NgIf],
  template: `
    <div
      class="skeleton"
      [ngClass]="'skeleton--' + variant()"
      [style.width]="width()"
      [style.height]="height()"
      role="status"
      aria-label="Carregando"
    >
      <ng-container *ngIf="variant() === 'text'">
        <div class="skeleton__line" *ngFor="let _ of linesArray"></div>
      </ng-container>
      <ng-container *ngIf="variant() === 'table'">
        <div class="skeleton__header"></div>
        <div class="skeleton__row" *ngFor="let _ of rowsArray"></div>
      </ng-container>
    </div>
  `,
  styles: [
    `
      .skeleton {
        display: flex;
        flex-direction: column;
        gap: 8px;
        border-radius: 10px;
        overflow: hidden;
        position: relative;
        background: color-mix(in srgb, var(--color-muted) 40%, transparent);
      }

      .skeleton::after {
        content: '';
        position: absolute;
        inset: 0;
        transform: translateX(-100%);
        background: linear-gradient(
          90deg,
          transparent,
          color-mix(in srgb, var(--color-foreground) 8%, transparent),
          transparent
        );
        animation: shimmer 1.4s infinite;
      }

      @keyframes shimmer {
        100% {
          transform: translateX(100%);
        }
      }

      .skeleton--card {
        border: 1px solid var(--color-border);
      }

      .skeleton--text {
        background: transparent;
        justify-content: center;
      }

      .skeleton__line {
        height: 14px;
        border-radius: 7px;
        background: color-mix(in srgb, var(--color-muted) 45%, transparent);
      }

      .skeleton--table {
        background: transparent;
        border: 1px solid var(--color-border);
        padding: 10px;
      }

      .skeleton__header {
        height: 32px;
        border-radius: 6px;
        background: color-mix(in srgb, var(--color-muted) 55%, transparent);
      }

      .skeleton__row {
        height: 24px;
        border-radius: 6px;
        background: color-mix(in srgb, var(--color-muted) 35%, transparent);
      }

      .skeleton--chart {
        border: 1px solid var(--color-border);
      }
    `
  ]
})
export class SkeletonComponent {
  variant = input<SkeletonVariant>('card');
  width = input<string>('100%');
  height = input<string>('200px');
  lines = input<number>(3);
  rows = input<number>(5);

  get linesArray(): number[] {
    return Array(this.lines()).fill(0);
  }

  get rowsArray(): number[] {
    return Array(this.rows()).fill(0);
  }
}
