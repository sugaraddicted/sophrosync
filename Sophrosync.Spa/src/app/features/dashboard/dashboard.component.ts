import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  template: `
    <div>
      <h1>Welcome, {{ profile()?.firstName ?? 'User' }}</h1>
      <p>Roles: {{ roles().join(', ') }}</p>
      <button (click)="auth.logout()">Sign out</button>
    </div>
  `,
})
export class DashboardComponent {
  protected readonly auth = inject(AuthService);
  protected readonly profile = this.auth.userProfile;
  protected readonly roles = this.auth.userRoles;
}
