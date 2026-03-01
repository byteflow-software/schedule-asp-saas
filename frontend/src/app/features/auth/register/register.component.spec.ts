import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../core/services/auth.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authServiceSpy: { register: jest.Mock; login: jest.Mock };
  let router: Router;
  let snackBarSpy: { open: jest.Mock };

  beforeEach(async () => {
    authServiceSpy = { register: jest.fn(), login: jest.fn() };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
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

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have all required form fields', () => {
    expect(component.form.controls.tenantName).toBeDefined();
    expect(component.form.controls.fullName).toBeDefined();
    expect(component.form.controls.email).toBeDefined();
    expect(component.form.controls.password).toBeDefined();
  });

  it('should be invalid when empty', () => {
    expect(component.form.invalid).toBe(true);
  });

  it('should be valid with all fields filled correctly', () => {
    component.form.patchValue({ tenantName: 'Co', fullName: 'John', email: 'j@t.com', password: 'password1' });
    expect(component.form.valid).toBe(true);
  });

  it('should require password minimum length of 8', () => {
    component.form.controls.password.setValue('short');
    expect(component.form.controls.password.hasError('minlength')).toBe(true);
  });

  it('should not call register when form is invalid', () => {
    component.onSubmit();
    expect(authServiceSpy.register).not.toHaveBeenCalled();
  });

  it('should start with hidePassword true', () => {
    expect(component.hidePassword).toBe(true);
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.form.patchValue({ tenantName: 'Co', fullName: 'John', email: 'j@t.com', password: 'password1' });
    });

    it('should register and auto-login on success, navigate to dashboard', () => {
      authServiceSpy.register.mockReturnValue(of({ tenantId: 't1', tokens: { accessToken: 'a', refreshToken: 'r' } }));
      authServiceSpy.login.mockReturnValue(of({ userId: 'u1' }));

      component.onSubmit();

      expect(authServiceSpy.register).toHaveBeenCalledWith(expect.objectContaining({ tenantName: 'Co', email: 'j@t.com' }));
      expect(authServiceSpy.login).toHaveBeenCalledWith({ email: 'j@t.com', password: 'password1' });
      expect(snackBarSpy.open).toHaveBeenCalledWith('Conta criada com sucesso!', 'OK', expect.any(Object));
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should navigate to login when auto-login fails', () => {
      authServiceSpy.register.mockReturnValue(of({ tenantId: 't1' }));
      authServiceSpy.login.mockReturnValue(throwError(() => new Error('fail')));

      component.onSubmit();

      expect(snackBarSpy.open).toHaveBeenCalledWith('Conta criada! Faça login.', 'OK', expect.any(Object));
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should reset loading on register error', () => {
      authServiceSpy.register.mockReturnValue(throwError(() => new Error('fail')));

      component.onSubmit();

      expect(component.loading()).toBe(false);
      expect(authServiceSpy.login).not.toHaveBeenCalled();
    });
  });
});
