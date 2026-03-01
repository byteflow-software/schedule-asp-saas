import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { AsaasSetupDialogComponent } from './asaas-setup-dialog.component';
import { TenantService } from '../../../core/services/tenant.service';

describe('AsaasSetupDialogComponent', () => {
  let component: AsaasSetupDialogComponent;
  let fixture: ComponentFixture<AsaasSetupDialogComponent>;
  let dialogRefSpy: { close: jest.Mock };
  let tenantServiceSpy: { validateAsaasKey: jest.Mock };
  let router: Router;

  beforeEach(async () => {
    dialogRefSpy = { close: jest.fn() };
    tenantServiceSpy = { validateAsaasKey: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [AsaasSetupDialogComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: TenantService, useValue: tenantServiceSpy },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(AsaasSetupDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty apiKey', () => {
    expect(component.apiKey).toBe('');
  });

  it('should start with showToken false', () => {
    expect(component.showToken()).toBe(false);
  });

  describe('validate', () => {
    it('should return early if apiKey is empty', () => {
      component.apiKey = '';
      component.validate();
      expect(tenantServiceSpy.validateAsaasKey).not.toHaveBeenCalled();
    });

    it('should set success on valid key', () => {
      tenantServiceSpy.validateAsaasKey.mockReturnValue(of(void 0));
      component.apiKey = '$aact_test';

      component.validate();

      expect(tenantServiceSpy.validateAsaasKey).toHaveBeenCalledWith('$aact_test');
      expect(component.validating()).toBe(false);
      expect(component.success()).toBe(true);
      expect(component.error()).toBe('');
    });

    it('should set error on invalid key', () => {
      tenantServiceSpy.validateAsaasKey.mockReturnValue(throwError(() => new Error('fail')));
      component.apiKey = 'bad-key';

      component.validate();

      expect(component.validating()).toBe(false);
      expect(component.success()).toBe(false);
      expect(component.error()).toBe('Token inválido. Verifique se copiou corretamente.');
    });

    it('should clear previous error before validating', () => {
      component.error.set('previous error');
      tenantServiceSpy.validateAsaasKey.mockReturnValue(of(void 0));
      component.apiKey = '$aact_test';

      component.validate();

      expect(component.error()).toBe('');
    });
  });

  describe('goToSettings', () => {
    it('should close dialog and navigate to settings', () => {
      component.goToSettings();
      expect(dialogRefSpy.close).toHaveBeenCalledWith(false);
      expect(router.navigate).toHaveBeenCalledWith(['/settings']);
    });
  });
});
