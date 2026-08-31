import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HttpInterceptorFn, provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';

import { KeycloakService } from 'keycloak-angular';
import { initializeKeycloak } from './core/auth/keycloak-init.factory';
import { KeycloakSessionBridgeService } from './core/auth/keycloak-session-bridge';
import { httpErrorInterceptor } from './core/interceptors/http-error.interceptor';

function initializeSessionBridge(
  keycloak: KeycloakService,
  bridge: KeycloakSessionBridgeService
) {
  return () => bridge.start();
}

const keycloakBearerInterceptor: HttpInterceptorFn = (req, next) => {
  const keycloak = inject(KeycloakService);
  if (keycloak.isLoggedIn()) {
    const token = keycloak.getToken();
    if (token) {
      const cloned = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
      return next(cloned);
    }
  }
  return next(req);
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([keycloakBearerInterceptor, httpErrorInterceptor])),
    provideAnimationsAsync(),
    provideEchartsCore({ echarts }),
    KeycloakService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeKeycloak,
      multi: true,
      deps: [KeycloakService]
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeSessionBridge,
      multi: true,
      deps: [KeycloakService, KeycloakSessionBridgeService]
    }
  ]
};
