import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

// Spec ref: Architecture Spec Section 1 — OIDC Authorization Code + PKCE redirect handler.
// Keycloak redirects to /callback after successful authentication. This component
// triggers the code exchange and then navigates to the protected area.
@Component({
  selector: 'app-callback',
  standalone: true,
  template: `<p>Completing sign-in...</p>`,
})
export class CallbackComponent implements OnInit {
  private readonly oidc = inject(OidcSecurityService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.oidc.checkAuth().subscribe(({ isAuthenticated }) => {
      if (isAuthenticated) {
        this.router.navigate(['/dashboard']);
      } else {
        this.router.navigate(['/login']);
      }
    });
  }
}
