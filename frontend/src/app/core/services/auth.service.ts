import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RegisterResponse,
  TokenPair,
  DecodedToken,
} from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;
  private readonly TOKEN_KEY = 'scheduly_access_token';
  private readonly REFRESH_KEY = 'scheduly_refresh_token';
  private readonly USER_KEY = 'scheduly_user';

  private currentUserSignal = signal<AuthResponse | null>(this.loadUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.currentUserSignal());
  readonly isAdmin = computed(() => this.currentUserSignal()?.role === 'Admin');
  readonly tenantId = computed(() => this.currentUserSignal()?.tenantId ?? '');

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => this.handleAuthSuccess(response))
    );
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, request);
  }

  refreshToken(): Observable<TokenPair | null> {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    if (!refreshToken) return of(null);
    return this.http
      .post<TokenPair>(`${this.apiUrl}/refresh-token`, { token: refreshToken })
      .pipe(
        tap((tokens) => {
          localStorage.setItem(this.TOKEN_KEY, tokens.accessToken);
          localStorage.setItem(this.REFRESH_KEY, tokens.refreshToken);
        }),
        catchError(() => {
          this.logout();
          return of(null);
        })
      );
  }

  logout(): void {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY);
    if (refreshToken) {
      this.http
        .post(`${this.apiUrl}/revoke-token`, { token: refreshToken })
        .subscribe();
    }
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSignal.set(null);
    this.router.navigate(['/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as DecodedToken;
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.tokens.accessToken);
    localStorage.setItem(this.REFRESH_KEY, response.tokens.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(response));
    this.currentUserSignal.set(response);
  }

  handleRegisterSuccess(response: RegisterResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.tokens.accessToken);
    localStorage.setItem(this.REFRESH_KEY, response.tokens.refreshToken);
  }

  private loadUser(): AuthResponse | null {
    const stored = localStorage.getItem(this.USER_KEY);
    if (!stored) return null;
    try {
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }
}
