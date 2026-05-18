import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard, roleGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { signal } from '@angular/core';

function runGuard(guard: ReturnType<typeof authGuard | typeof roleGuard>) {
  return TestBed.runInInjectionContext(() =>
    (guard as (r: ActivatedRouteSnapshot, s: RouterStateSnapshot) => boolean | UrlTree)(
      {} as ActivatedRouteSnapshot,
      {} as RouterStateSnapshot,
    )
  );
}

describe('authGuard', () => {
  let auth: AuthService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
      ],
    });
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  it('returns true when authenticated', () => {
    auth.isAuthenticated.set(true);

    const result = runGuard(authGuard);

    expect(result).toBe(true);
  });

  it('returns UrlTree to /login when not authenticated', () => {
    auth.isAuthenticated.set(false);

    const result = runGuard(authGuard);

    expect(result instanceof UrlTree).toBe(true);
    expect(router.serializeUrl(result as UrlTree)).toBe('/login');
  });
});

describe('roleGuard', () => {
  let auth: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
      ],
    });
    auth = TestBed.inject(AuthService);
  });

  it('returns true when authenticated and has the required role', () => {
    auth.isAuthenticated.set(true);
    auth.userRoles.set(['therapist', 'admin']);

    const result = runGuard(roleGuard('therapist'));

    expect(result).toBe(true);
  });

  it('returns false when authenticated but missing the required role', () => {
    auth.isAuthenticated.set(true);
    auth.userRoles.set(['receptionist']);

    const result = runGuard(roleGuard('therapist'));

    expect(result).toBe(false);
  });

  it('returns false when not authenticated even if role matches', () => {
    auth.isAuthenticated.set(false);
    auth.userRoles.set(['therapist']);

    const result = runGuard(roleGuard('therapist'));

    expect(result).toBe(false);
  });

  it('returns false when not authenticated and no roles', () => {
    auth.isAuthenticated.set(false);
    auth.userRoles.set([]);

    const result = runGuard(roleGuard('therapist'));

    expect(result).toBe(false);
  });
});
