import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { of } from 'rxjs';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let authServiceMock: {
    isAuthenticated: jest.Mock;
    isTokenExpired: jest.Mock;
    refreshToken: jest.Mock;
    logout: jest.Mock;
  };
  let routerMock: { navigate: jest.Mock };

  const mockRoute = {} as ActivatedRouteSnapshot;
  const mockState = {} as RouterStateSnapshot;

  beforeEach(() => {
    authServiceMock = {
      isAuthenticated: jest.fn(),
      isTokenExpired: jest.fn(),
      refreshToken: jest.fn(),
      logout: jest.fn(),
    };

    routerMock = {
      navigate: jest.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    });
  });

  it('should return true when user is authenticated and token is not expired', () => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.isTokenExpired.mockReturnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(true);
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should navigate to /login and return false when not authenticated', () => {
    authServiceMock.isAuthenticated.mockReturnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should try to refresh token when token is expired and return true on success', (done) => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.isTokenExpired.mockReturnValue(true);
    authServiceMock.refreshToken.mockReturnValue(of({ accessToken: 'new', refreshToken: 'new' }));

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    // Result is an Observable when token needs refresh
    (result as any).subscribe((value: boolean) => {
      expect(value).toBe(true);
      done();
    });
  });

  it('should logout when token refresh returns null', (done) => {
    authServiceMock.isAuthenticated.mockReturnValue(true);
    authServiceMock.isTokenExpired.mockReturnValue(true);
    authServiceMock.refreshToken.mockReturnValue(of(null));

    const result = TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    (result as any).subscribe((value: boolean) => {
      expect(value).toBe(false);
      expect(authServiceMock.logout).toHaveBeenCalled();
      done();
    });
  });
});
