import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly fallbackClaims = ['CanViewDashboard', 'CanViewENA', 'CanSimulate'];

  private get userClaims(): string[] {
    const rolesRaw = sessionStorage.getItem('energysuite_roles');
    if (rolesRaw) {
      try {
        const roles = JSON.parse(rolesRaw);
        if (Array.isArray(roles) && roles.length) {
          return roles;
        }
      } catch {
        // fallthrough to claims parsing
      }
    }

    const claimsRaw = sessionStorage.getItem('energysuite_claims');
    if (claimsRaw) {
      try {
        const claims = JSON.parse(claimsRaw);
        const realmRoles: string[] = claims?.realm_access?.roles ?? [];
        const clientRoles: string[] =
          claims?.resource_access?.['energysuite-frontend']?.roles ?? [];
        return Array.from(new Set([...realmRoles, ...clientRoles]));
      } catch {
        // fallthrough to fallback
      }
    }

    return this.fallbackClaims;
  }

  hasPermission(claim: string): boolean {
    return this.userClaims.includes(claim);
  }

  isAuthenticated(): boolean {
    return !!sessionStorage.getItem('energysuite_token');
  }

  getToken(): string | null {
    return sessionStorage.getItem('energysuite_token');
  }
}
