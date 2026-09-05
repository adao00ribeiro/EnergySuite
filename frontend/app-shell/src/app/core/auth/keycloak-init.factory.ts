import { inject } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationService } from '../services/notification.service';

const KEYCLOAK_TIMEOUT_MS = 15000;

export function initializeKeycloak(keycloak: KeycloakService) {
  const notification = inject(NotificationService);
  return () =>
    withTimeout(
      keycloak.init({
        config: {
          url: '/auth',
          realm: 'EnergySuite',
          clientId: 'energysuite-frontend'
        },
        initOptions: {
          onLoad: 'login-required',
          silentCheckSsoRedirectUri:
            window.location.origin + '/assets/silent-check-sso.html',
          pkceMethod: 'S256',
          checkLoginIframe: false
        },
        bearerExcludedUrls: ['/assets']
      }),
      KEYCLOAK_TIMEOUT_MS
    )
      .then(() => undefined)
      .catch((err) => {
        console.error('Erro ao inicializar Keycloak:', err);
        notification.error(
          'Não foi possível conectar ao servidor de autenticação. Verifique sua conexão.'
        );
        sessionStorage.setItem(
          'energysuite_init_error',
          'Não foi possível conectar ao servidor de autenticação.'
        );
        throw err;
      });
}

function withTimeout<T>(promise: Promise<T>, ms: number): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    const timer = setTimeout(
      () => reject(new Error(`Timeout aguardando resposta do Keycloak (${ms}ms).`)),
      ms
    );
    promise.then(
      (value) => {
        clearTimeout(timer);
        resolve(value);
      },
      (err) => {
        clearTimeout(timer);
        reject(err);
      }
    );
  });
}
