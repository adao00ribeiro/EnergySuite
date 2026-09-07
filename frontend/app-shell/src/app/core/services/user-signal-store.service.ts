import { Injectable, signal, computed } from '@angular/core';

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  tenantId: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserSignalStore {
  // Reactive Signals for user state
  readonly currentUser = signal<UserProfile | null>({
    id: 'user-001',
    name: 'Analista de Riscos & Trading',
    email: 'trader@energysuite.com.br',
    tenantId: '00000000-0000-0000-0000-000000000001',
    roles: ['Trader', 'RiskAnalyst', 'CceeOperator']
  });

  // Computed State
  readonly isAuthenticated = computed(() => !!this.currentUser()?.id);
  readonly activeTenantId = computed(() => this.currentUser()?.tenantId ?? '');
  readonly userRoles = computed(() => this.currentUser()?.roles ?? []);

  updateUser(profile: UserProfile): void {
    this.currentUser.set(profile);
    localStorage.setItem('energysuite_user', JSON.stringify(profile));
  }

  hasRole(role: string): boolean {
    return this.userRoles().includes(role);
  }

  clearUser(): void {
    this.currentUser.set(null);
    localStorage.removeItem('energysuite_user');
  }
}
