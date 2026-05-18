import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

const TOKEN_URL = `${environment.keycloak.url}/realms/${environment.keycloak.realm}/protocol/openid-connect/token`;

function makeJwt(payload: object): string {
  const header = btoa(JSON.stringify({ alg: 'RS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `${header}.${body}.fakesig`;
}

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        AuthService,
      ],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  describe('initial state', () => {
    it('starts unauthenticated', () => {
      expect(service.isAuthenticated()).toBe(false);
      expect(service.userProfile()).toBeNull();
      expect(service.userRoles()).toEqual([]);
    });
  });

  describe('login with valid JWT', () => {
    it('sets isAuthenticated to true and populates profile', async () => {
      const loginPromise = service.login('therapist', 'secret');

      const req = http.expectOne(TOKEN_URL);
      req.flush({
        access_token: makeJwt({
          preferred_username: 'therapist',
          email: 'therapist@clinic.com',
          given_name: 'Jane',
          family_name: 'Smith',
          roles: ['therapist'],
        }),
        refresh_token: 'rt-valid',
        expires_in: 300,
      });

      await loginPromise;

      expect(service.isAuthenticated()).toBe(true);
      expect(service.userProfile()?.username).toBe('therapist');
      expect(service.userRoles()).toContain('therapist');
    });

    it('stores refresh token in sessionStorage', async () => {
      const loginPromise = service.login('user', 'pass');

      http.expectOne(TOKEN_URL).flush({
        access_token: makeJwt({ preferred_username: 'user', email: '', given_name: '', family_name: '' }),
        refresh_token: 'stored-rt',
        expires_in: 300,
      });

      await loginPromise;

      expect(sessionStorage.getItem('sophrosync_rt')).toBe('stored-rt');
    });

    it('reads roles from realm_access when roles claim absent', async () => {
      const loginPromise = service.login('user', 'pass');

      http.expectOne(TOKEN_URL).flush({
        access_token: makeJwt({
          preferred_username: 'user',
          email: '',
          given_name: '',
          family_name: '',
          realm_access: { roles: ['receptionist'] },
        }),
        refresh_token: 'rt',
        expires_in: 300,
      });

      await loginPromise;

      expect(service.userRoles()).toContain('receptionist');
    });
  });

  describe('login with malformed JWT', () => {
    it('does not authenticate when JWT has wrong number of parts', async () => {
      const loginPromise = service.login('user', 'pass');

      http.expectOne(TOKEN_URL).flush({
        access_token: 'only.twoparts',
        refresh_token: 'rt',
        expires_in: 300,
      });

      await loginPromise;

      expect(service.isAuthenticated()).toBe(false);
      expect(sessionStorage.getItem('sophrosync_rt')).toBeNull();
    });

    it('does not authenticate when JWT payload is invalid base64', async () => {
      const loginPromise = service.login('user', 'pass');

      http.expectOne(TOKEN_URL).flush({
        access_token: 'header.!!!invalid_base64!!!.sig',
        refresh_token: 'rt',
        expires_in: 300,
      });

      await loginPromise;

      expect(service.isAuthenticated()).toBe(false);
      expect(sessionStorage.getItem('sophrosync_rt')).toBeNull();
    });

    it('does not authenticate when JWT payload is not valid JSON', async () => {
      const notJson = btoa('not json at all');
      const loginPromise = service.login('user', 'pass');

      http.expectOne(TOKEN_URL).flush({
        access_token: `header.${notJson}.sig`,
        refresh_token: 'rt',
        expires_in: 300,
      });

      await loginPromise;

      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('logout', () => {
    it('clears authentication state and removes refresh token', async () => {
      const router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);

      // First login
      const loginPromise = service.login('user', 'pass');
      http.expectOne(TOKEN_URL).flush({
        access_token: makeJwt({ preferred_username: 'user', email: '', given_name: '', family_name: '' }),
        refresh_token: 'rt',
        expires_in: 300,
      });
      await loginPromise;

      await service.logout();

      expect(service.isAuthenticated()).toBe(false);
      expect(service.userProfile()).toBeNull();
      expect(sessionStorage.getItem('sophrosync_rt')).toBeNull();
    });
  });
});
