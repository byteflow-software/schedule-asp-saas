import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: { login: jest.Mock };
  let router: Router;
  let snackBarSpy: { open: jest.Mock };

  beforeEach(async () => {
    authServiceSpy = { login: jest.fn() };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have email and password form controls', () => {
    expect(component.form.controls.email).toBeDefined();
    expect(component.form.controls.password).toBeDefined();
  });

  it('should be invalid when empty', () => {
    expect(component.form.invalid).toBe(true);
  });

  it('should be valid with correct values', () => {
    component.form.patchValue({ email: 'test@example.com', password: 'pass' });
    expect(component.form.valid).toBe(true);
  });

  it('should start with hidePassword true', () => {
    expect(component.hidePassword).toBe(true);
  });

  it('should not call login when form is invalid', () => {
    component.onSubmit();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.form.patchValue({ email: 'test@example.com', password: 'password123' });
    });

    it('should call authService.login and navigate to dashboard on success', () => {
      authServiceSpy.login.mockReturnValue(of({ userId: '1' }));

      component.onSubmit();

      expect(authServiceSpy.login).toHaveBeenCalledWith({ email: 'test@example.com', password: 'password123' });
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should show 401-specific error message', () => {
      authServiceSpy.login.mockReturnValue(throwError(() => ({ status: 401 })));

      component.onSubmit();

      expect(snackBarSpy.open).toHaveBeenCalledWith('Email ou senha incorretos.', 'OK', expect.any(Object));
      expect(component.loading()).toBe(false);
    });

    it('should show generic error message for non-401 errors', () => {
      authServiceSpy.login.mockReturnValue(throwError(() => ({ status: 500 })));

      component.onSubmit();

      expect(snackBarSpy.open).toHaveBeenCalledWith('Erro ao fazer login. Tente novamente.', 'OK', expect.any(Object));
      expect(component.loading()).toBe(false);
    });
  });
});
