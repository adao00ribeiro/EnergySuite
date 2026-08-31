import { Component, input, output } from '@angular/core';
import { NgIf } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-error-fallback',
  standalone: true,
  imports: [NgIf, MatIconModule, MatButtonModule],
  template: `
    <div class="error-fallback">
      <div class="error-fallback__icon" aria-hidden="true">
        <mat-icon>error_outline</mat-icon>
      </div>
      <h1 class="error-fallback__title">{{ title() }}</h1>
      <p class="error-fallback__message">{{ message() }}</p>
      <code class="error-fallback__code" *ngIf="code()">Erro {{ code() }}</code>
      <div class="error-fallback__actions">
        <button mat-flat-button color="primary" (click)="retry.emit()">
          <mat-icon>refresh</mat-icon>
          Tentar Novamente
        </button>
        <button mat-stroked-button (click)="reload()">
          Recarregar Página
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 100%;
        width: 100%;
        padding: 32px;
        box-sizing: border-box;
      }

      .error-fallback {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        max-width: 420px;
        gap: 12px;
        background: var(--color-card);
        border: 1px solid var(--color-border);
        border-radius: 16px;
        padding: 40px 32px;
        box-shadow: var(--shadow-lg);
        animation: fadeIn 0.3s ease-out forwards;
      }

      .error-fallback__icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 72px;
        height: 72px;
        border-radius: 50%;
        background: color-mix(in srgb, var(--color-destructive) 15%, transparent);
        color: var(--color-destructive);
        animation: pulse 2s ease-in-out infinite;
      }

      .error-fallback__icon mat-icon {
        font-size: 40px;
        width: 40px;
        height: 40px;
      }

      .error-fallback__title {
        font-family: var(--font-display);
        font-size: 1.4rem;
        font-weight: 600;
        margin: 0;
        color: var(--color-foreground);
      }

      .error-fallback__message {
        color: var(--color-muted-foreground);
        font-size: 0.95rem;
        margin: 0;
        line-height: 1.6;
      }

      .error-fallback__code {
        font-family: var(--font-display);
        font-size: 0.8rem;
        color: var(--color-muted-foreground);
        background: var(--color-muted);
        padding: 4px 10px;
        border-radius: 6px;
      }

      .error-fallback__actions {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        justify-content: center;
        margin-top: 12px;
      }

      .error-fallback__actions button {
        display: inline-flex;
        align-items: center;
        gap: 6px;
      }

      @keyframes pulse {
        0%, 100% { transform: scale(1); }
        50% { transform: scale(1.06); }
      }

      @keyframes fadeIn {
        from { opacity: 0; transform: translateY(8px); }
        to { opacity: 1; transform: translateY(0); }
      }
    `
  ]
})
export class ErrorFallbackComponent {
  title = input<string>('Serviço Indisponível');
  message = input<string>(
    'Não foi possível conectar ao serviço. Tente novamente em alguns instantes.'
  );
  code = input<string | null>(null);

  readonly retry = output<void>();

  reload() {
    window.location.reload();
  }
}
