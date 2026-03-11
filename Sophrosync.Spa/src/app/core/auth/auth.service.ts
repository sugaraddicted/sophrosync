import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserProfile {
  username: string;
  email: string;
  firstName: string;
  lastName: string;
}

interface TokenResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
}

interface JwtPayload {
  preferred_username: string;
  email: string;
  given_name: string;
  family_name: string;
  roles?: string[];
  realm_access?: { roles: string[] };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenUrl = `${environment.keycloak.url}/realms/${environment.keycloak.realm}/protocol/openid-connect/token`;

  private accessToken: string | null = null;
  private tokenExpiry = 0;

  readonly isAuthenticated = signal(false);
  readonly userProfile = signal<UserProfile | null>(null);
  readonly userRoles = signal<string[]>([]);

  async login(username: string, password: string): Promise<void> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('client_id', environment.keycloak.clientId)
      .set('username', username)
      .set('password', password);

    const tokens = await firstValueFrom(
      this.http.post<TokenResponse>(this.tokenUrl, body.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      })
    );

    this.applyTokens(tokens);
  }

  async logout(): Promise<void> {
    sessionStorage.removeItem('sophrosync_rt');
    this.accessToken = null;
    this.isAuthenticated.set(false);
    this.userProfile.set(null);
    this.userRoles.set([]);
    await this.router.navigate(['/login']);
  }

  async getToken(): Promise<string | null> {
    if (this.accessToken && Date.now() < this.tokenExpiry) {
      return this.accessToken;
    }
    return this.tryRefresh();
  }

  /** Called on app init — restores session from stored refresh token. */
  async restoreSession(): Promise<void> {
    await this.tryRefresh();
  }

  private async tryRefresh(): Promise<string | null> {
    const rt = sessionStorage.getItem('sophrosync_rt');
    if (!rt) return null;

    try {
      const body = new HttpParams()
        .set('grant_type', 'refresh_token')
        .set('client_id', environment.keycloak.clientId)
        .set('refresh_token', rt);

      const tokens = await firstValueFrom(
        this.http.post<TokenResponse>(this.tokenUrl, body.toString(), {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        })
      );

      this.applyTokens(tokens);
      return this.accessToken;
    } catch {
      sessionStorage.removeItem('sophrosync_rt');
      return null;
    }
  }

  private applyTokens(tokens: TokenResponse): void {
    this.accessToken = tokens.access_token;
    this.tokenExpiry = Date.now() + (tokens.expires_in - 30) * 1000;
    sessionStorage.setItem('sophrosync_rt', tokens.refresh_token);

    const payload = this.decodeJwt(tokens.access_token);
    this.userProfile.set({
      username: payload.preferred_username,
      email: payload.email ?? '',
      firstName: payload.given_name ?? '',
      lastName: payload.family_name ?? '',
    });
    this.userRoles.set(payload.roles ?? payload.realm_access?.roles ?? []);
    this.isAuthenticated.set(true);
  }

  private decodeJwt(token: string): JwtPayload {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
  }
}
