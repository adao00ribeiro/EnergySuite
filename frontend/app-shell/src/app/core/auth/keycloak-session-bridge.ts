import { Injectable, inject } from '@angular/core';
import { KeycloakEventType } from 'keycloak-angular';
import { KeycloakService } from 'keycloak-angular';

const TOKEN_KEY = 'energysuite_token';
const CLAIMS_KEY = 'energysuite_claims';
const ROLES_KEY = 'energysuite_roles';

@Injectable({
  providedIn: 'root'
})
export class KeycloakSessionBridgeService {
  private keycloak = inject(KeycloakService);

  start(): void {
    this.sync();
    this.keycloak.keycloakEvents$.subscribe((event) => {
      if (event.type === KeycloakEventType.OnAuthRefreshSuccess) {
        this.sync();
      }
      if (event.type === KeycloakEventType.OnAuthLogout) {
        this.clear();
      }
    });
  }

  private sync(): void {
    const instance = this.keycloak.getKeycloakInstance();
    const tokenParsed = instance.tokenParsed as any;

    if (instance.token) {
      sessionStorage.setItem(TOKEN_KEY, instance.token);
    } else {
      sessionStorage.removeItem(TOKEN_KEY);
    }

    if (tokenParsed) {
      sessionStorage.setItem(CLAIMS_KEY, JSON.stringify(tokenParsed));
    } else {
      sessionStorage.removeItem(CLAIMS_KEY);
    }

    const realmRoles: string[] = tokenParsed?.realm_access?.roles ?? [];
    const clientRoles: string[] =
      tokenParsed?.resource_access?.['energysuite-frontend']?.roles ?? [];
    const roles = Array.from(new Set([...realmRoles, ...clientRoles]));
    sessionStorage.setItem(ROLES_KEY, JSON.stringify(roles));
  }

  private clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(CLAIMS_KEY);
    sessionStorage.removeItem(ROLES_KEY);
  }
}
