import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  // Mock roles/claims from a simulated Keycloak JWT token
  private userClaims: string[] = ['CanViewDashboard', 'CanViewENA', 'CanSimulate'];

  constructor() { }

  /**
   * Checks if the active user has a specific permission/claim.
   */
  hasPermission(claim: string): boolean {
    return this.userClaims.includes(claim);
  }

  /**
   * Helper function for UI toggle mock testing
   */
  togglePermission(claim: string) {
    if (this.hasPermission(claim)) {
      this.userClaims = this.userClaims.filter(c => c !== claim);
    } else {
      this.userClaims.push(claim);
    }
  }
}
