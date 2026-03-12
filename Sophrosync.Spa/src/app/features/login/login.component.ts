import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

// Spec ref: Architecture Spec Section 1 — login is handled entirely by Keycloak.
// The SPA never collects or processes credentials — it delegates to Keycloak via PKCE redirect.
// OWASP A07 (Auth Failures): no in-app credential handling, no ROPC grant_type=password.
@Component({
  selector: 'app-login',
  standalone: true,
  template: `
    <div class="login-container">
      <h1>Sophrosync</h1>
      <p>Sign in with your practice account.</p>
      <button (click)="signIn()">Sign in</button>
    </div>
  `,
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);

  signIn(): void {
    this.auth.login();
  }
}
