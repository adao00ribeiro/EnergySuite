import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { timeout, catchError, throwError, TimeoutError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

const MESSAGES: Record<string, string> = {
  '408': 'Requisição expirada. Tente novamente.',
  '503': 'Serviço em manutenção. Tente novamente em alguns minutos.',
  '504': 'Servidor demorou a responder. Tente novamente.',
  '500': 'Erro interno do servidor.',
  '502': 'Gateway com erro. Tente novamente.',
  '0': 'Serviço temporariamente indisponível. Verifique sua conexão.'
};

const DEFAULT_MESSAGE = 'Serviço temporariamente indisponível. Verifique sua conexão.';

const REQUEST_TIMEOUT_MS = 15000;

const lastNotified = new Map<string, number>();

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notification = inject(NotificationService);

  return next(req).pipe(
    timeout(REQUEST_TIMEOUT_MS),
    catchError((error: unknown) => {
      const skip =
        error instanceof HttpErrorResponse &&
        (error.status === 401 || error.status === 403);

      if (!skip) {
        const message = resolveMessage(error);
        const now = Date.now();
        const last = lastNotified.get(req.url) ?? 0;
        if (now - last > 3000) {
          lastNotified.set(req.url, now);
          notification.error(message);
        }
      }
      return throwError(() => error);
    })
  );
};

function resolveMessage(error: unknown): string {
  if (error instanceof TimeoutError) {
    return MESSAGES['504'];
  }
  if (error instanceof HttpErrorResponse) {
    return MESSAGES[String(error.status)] ?? DEFAULT_MESSAGE;
  }
  return DEFAULT_MESSAGE;
}
