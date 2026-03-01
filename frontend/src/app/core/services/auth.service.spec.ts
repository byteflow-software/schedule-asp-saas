import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthResponse, RegisterResponse } from '../models/auth.model';

function clearAuthStorage(): void {
  localStorage.removeItem('scheduly_access_token');
  localStorage.removeItem('scheduly_refresh_token');
  localStorage.removeItem('scheduly_user');
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let routerSpy: { navigate: jest.Mock };

  const mockAuthResponse: AuthResponse = {
    userId: 'u1',
    tenantId: 't1',
    fullName: 'John Doe',
    role: 'Admin',
    tokens: {
      accessToken: 'access-token-123',
      refreshToken: 'refresh-token-456',
      expiresAt: '2026-12-31T23:59:59Z',
    },
  };

  const mockRegisterResponse: RegisterResponse = {
    tenantId: 't1',
    userId: 'u1',
    tokens: {
      accessToken: 'access-token-789',
      refreshToken: 'refresh-token-012',
      expiresAt: '2026-12-31T23:59:59Z',
    },
  };

  beforeEach(() => {
    clearAuthStorage();
    routerSpy = { navigate: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: routerSpy },
      ],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    clearAuthStorage();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should POST login credentials and store auth data', () => {
      service.login({ email: 'john@test.com', password: 'Pass123!' }).subscribe((result) => {
        expect(result).toEqual(mockAuthResponse);
      });

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.method).toBe('POST');
      req.flush(mockAuthResponse);

      expect(localStorage.getItem('scheduly_access_token')).toBe('access-token-123');
      expect(localStorage.getItem('scheduly_refresh_token')).toBe('refresh-token-456');
      expect(localStorage.getItem('scheduly_user')).toBe(JSON.stringify(mockAuthResponse));
    });

    it('should update signals after login', () => {
      service.login({ email: 'j@t.com', password: 'P!' }).subscribe();
      httpMock.expectOne('/api/auth/login').flush(mockAuthResponse);

      expect(service.currentUser()).toEqual(mockAuthResponse);
      expect(service.isAuthenticated()).toBe(true);
      expect(service.isAdmin()).toBe(true);
      expect(service.tenantId()).toBe('t1');
    });
  });

  describe('register', () => {
    it('should POST registration data', () => {
      service.register({ tenantName: 'Co', fullName: 'J', email: 'j@t.com', password: 'P!' }).subscribe((r) => {
        expect(r).toEqual(mockRegisterResponse);
      });
      httpMock.expectOne('/api/auth/register').flush(mockRegisterResponse);
    });
  });

  describe('handleRegisterSuccess', () => {
    it('should store tokens in localStorage', () => {
      service.handleRegisterSuccess(mockRegisterResponse);
      expect(localStorage.getItem('scheduly_access_token')).toBe('access-token-789');
      expect(localStorage.getItem('scheduly_refresh_token')).toBe('refresh-token-012');
    });
  });

  describe('refreshToken', () => {
    it('should return null when no refresh token', () => {
      service.refreshToken().subscribe((r) => expect(r).toBeNull());
    });

    it('should POST and update storage', () => {
      localStorage.setItem('scheduly_refresh_token', 'rt-456');
      const newTokens = { accessToken: 'new-a', refreshToken: 'new-r', expiresAt: '2027-01-01' };
      service.refreshToken().subscribe((r) => expect(r).toEqual(newTokens));
      const req = httpMock.expectOne('/api/auth/refresh-token');
      expect(req.request.body).toEqual({ token: 'rt-456' });
      req.flush(newTokens);
      expect(localStorage.getItem('scheduly_access_token')).toBe('new-a');
    });

    it('should logout on error', () => {
      localStorage.setItem('scheduly_refresh_token', 'rt-456');
      service.refreshToken().subscribe((r) => expect(r).toBeNull());
      httpMock.expectOne('/api/auth/refresh-token').error(new ProgressEvent('error'));
      // logout triggers revoke-token POST since refresh token exists in storage
      const revokeReq = httpMock.expectOne('/api/auth/revoke-token');
      revokeReq.flush(null);
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('getAccessToken', () => {
    it('should return token', () => {
      localStorage.setItem('scheduly_access_token', 'my-token');
      expect(service.getAccessToken()).toBe('my-token');
    });

    it('should return null when no token', () => {
      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('computed signals', () => {
    it('isAuthenticated false when no user', () => expect(service.isAuthenticated()).toBe(false));
    it('isAdmin false when no user', () => expect(service.isAdmin()).toBe(false));
    it('tenantId empty when no user', () => expect(service.tenantId()).toBe(''));
  });

  describe('logout', () => {
    it('should clear storage and navigate to login', () => {
      localStorage.setItem('scheduly_access_token', 'x');
      service.logout();
      expect(localStorage.getItem('scheduly_access_token')).toBeNull();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should revoke refresh token if exists', () => {
      localStorage.setItem('scheduly_refresh_token', 'rt-456');
      service.logout();
      const req = httpMock.expectOne('/api/auth/revoke-token');
      expect(req.request.body).toEqual({ token: 'rt-456' });
      req.flush(null);
    });

    it('should not revoke if no refresh token', () => {
      service.logout();
      httpMock.expectNone('/api/auth/revoke-token');
    });

    it('should set currentUser to null', () => {
      service.logout();
      expect(service.currentUser()).toBeNull();
    });
  });

  describe('isTokenExpired', () => {
    it('should return true when no token', () => expect(service.isTokenExpired()).toBe(true));

    it('should return true for expired token', () => {
      const payload = { exp: 1000 };
      localStorage.setItem('scheduly_access_token', `h.${btoa(JSON.stringify(payload))}.s`);
      expect(service.isTokenExpired()).toBe(true);
    });

    it('should return false for valid token', () => {
      const payload = { exp: Math.floor(Date.now() / 1000) + 3600 };
      localStorage.setItem('scheduly_access_token', `h.${btoa(JSON.stringify(payload))}.s`);
      expect(service.isTokenExpired()).toBe(false);
    });

    it('should return true for malformed token', () => {
      localStorage.setItem('scheduly_access_token', 'not-a-jwt');
      expect(service.isTokenExpired()).toBe(true);
    });
  });
});

describe('AuthService loadUser with invalid JSON', () => {
  afterEach(() => {
    clearAuthStorage();
  });

  it('should return null for invalid JSON in localStorage', () => {
    localStorage.setItem('scheduly_user', '{not-valid-json');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: jest.fn() } },
      ],
    });
    const freshService = TestBed.inject(AuthService);
    expect(freshService.currentUser()).toBeNull();
  });

  it('should parse valid JSON from localStorage', () => {
    const user = { userId: 'u1', tenantId: 't1', fullName: 'John', role: 'Admin', tokens: { accessToken: 'a', refreshToken: 'r', expiresAt: '' } };
    localStorage.setItem('scheduly_user', JSON.stringify(user));
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: jest.fn() } },
      ],
    });
    const freshService = TestBed.inject(AuthService);
    expect(freshService.currentUser()).toEqual(user);
  });
});
