import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';

import { KeycloakService, KeycloakBearerInterceptor } from 'keycloak-angular';
import { initializeKeycloak } from './core/auth/keycloak-init.factory';
import { KeycloakSessionBridgeService } from './core/auth/keycloak-session-bridge';

function initializeSessionBridge(
  keycloak: KeycloakService,
  bridge: KeycloakSessionBridgeService
) {
  return () => bridge.start();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
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
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: KeycloakBearerInterceptor,
      multi: true
    }
  ]
};
