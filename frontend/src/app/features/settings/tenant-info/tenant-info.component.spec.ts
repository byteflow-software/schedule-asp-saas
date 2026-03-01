import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { TenantInfoComponent } from './tenant-info.component';
import { TenantService } from '../../../core/services/tenant.service';

describe('TenantInfoComponent', () => {
  let component: TenantInfoComponent;
  let fixture: ComponentFixture<TenantInfoComponent>;
  let tenantServiceSpy: { getMyTenant: jest.Mock; updateTenant: jest.Mock; validateAsaasKey: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockTenant = {
    id: '1', name: 'Test Tenant', cpfCnpj: '12345678901', email: 'test@test.com',
    phone: '11999999999', address: 'Rua Test', addressNumber: '123', complement: '',
    neighborhood: 'Centro', city: 'Sao Paulo', state: 'SP', postalCode: '01001000',
    logoUrl: '', hasAsaasIntegration: false, asaasWalletId: null,
  };

  beforeEach(async () => {
    tenantServiceSpy = {
      getMyTenant: jest.fn().mockReturnValue(of(mockTenant)),
      updateTenant: jest.fn().mockReturnValue(of({ ...mockTenant, name: 'Updated' })),
      validateAsaasKey: jest.fn().mockReturnValue(of({ ...mockTenant, hasAsaasIntegration: true, asaasWalletId: 'wal_123' })),
    };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [TenantInfoComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: TenantService, useValue: tenantServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(TenantInfoComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form fields', () => {
    expect(component.form.contains('name')).toBe(true);
    expect(component.form.contains('cpfCnpj')).toBe(true);
    expect(component.form.contains('email')).toBe(true);
    expect(component.form.contains('phone')).toBe(true);
    expect(component.form.contains('address')).toBe(true);
    expect(component.form.contains('addressNumber')).toBe(true);
    expect(component.form.contains('complement')).toBe(true);
    expect(component.form.contains('neighborhood')).toBe(true);
    expect(component.form.contains('city')).toBe(true);
    expect(component.form.contains('state')).toBe(true);
    expect(component.form.contains('postalCode')).toBe(true);
    expect(component.form.contains('logoUrl')).toBe(true);
  });

  describe('ngOnInit', () => {
    it('should load tenant and populate form', () => {
      component.ngOnInit();
      expect(tenantServiceSpy.getMyTenant).toHaveBeenCalled();
      expect(component.tenant()).toEqual(mockTenant);
      expect(component.form.getRawValue().name).toBe('Test Tenant');
      expect(component.form.getRawValue().cpfCnpj).toBe('12345678901');
      expect(component.form.getRawValue().city).toBe('Sao Paulo');
      expect(component.loading()).toBe(false);
    });

    it('should handle load error', () => {
      tenantServiceSpy.getMyTenant.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.loading()).toBe(false);
      expect(component.tenant()).toBeNull();
    });

    it('should handle null optional fields', () => {
      tenantServiceSpy.getMyTenant.mockReturnValue(of({
        ...mockTenant, cpfCnpj: null, email: null, phone: null, address: null,
        addressNumber: null, complement: null, neighborhood: null, city: null,
        state: null, postalCode: null, logoUrl: null,
      }));
      component.ngOnInit();
      expect(component.form.getRawValue().cpfCnpj).toBe('');
      expect(component.form.getRawValue().email).toBe('');
      expect(component.form.getRawValue().city).toBe('');
    });
  });

  describe('saveInfo', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should return early if form is invalid', () => {
      component.form.patchValue({ name: '' });
      component.saveInfo();
      expect(tenantServiceSpy.updateTenant).not.toHaveBeenCalled();
    });

    it('should call updateTenant and show success snackbar', () => {
      component.saveInfo();
      expect(tenantServiceSpy.updateTenant).toHaveBeenCalled();
      expect(component.saving()).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Dados atualizados com sucesso!', 'OK',
        expect.objectContaining({ duration: 3000 }),
      );
    });

    it('should handle save error', () => {
      tenantServiceSpy.updateTenant.mockReturnValue(throwError(() => new Error('fail')));
      component.saveInfo();
      expect(component.saving()).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Erro ao atualizar dados.', 'OK',
        expect.objectContaining({ duration: 3000 }),
      );
    });
  });

  describe('validateAsaas', () => {
    it('should return early if no API key', () => {
      component.asaasApiKey = '';
      component.validateAsaas();
      expect(tenantServiceSpy.validateAsaasKey).not.toHaveBeenCalled();
    });

    it('should call validateAsaasKey and show success snackbar', () => {
      component.asaasApiKey = '$aact_test_123';
      component.validateAsaas();
      expect(tenantServiceSpy.validateAsaasKey).toHaveBeenCalledWith('$aact_test_123');
      expect(component.validatingAsaas()).toBe(false);
      expect(component.asaasApiKey).toBe('');
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Token Asaas validado com sucesso!', 'OK',
        expect.objectContaining({ duration: 3000 }),
      );
      expect(component.tenant()?.hasAsaasIntegration).toBe(true);
    });

    it('should handle validation error', () => {
      component.asaasApiKey = 'invalid-key';
      tenantServiceSpy.validateAsaasKey.mockReturnValue(throwError(() => new Error('invalid')));
      component.validateAsaas();
      expect(component.validatingAsaas()).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Token Asaas inválido.', 'OK',
        expect.objectContaining({ duration: 3000 }),
      );
    });
  });

  describe('showToken toggle', () => {
    it('should start as false', () => {
      expect(component.showToken()).toBe(false);
    });

    it('should toggle', () => {
      component.showToken.set(true);
      expect(component.showToken()).toBe(true);
    });
  });
});
