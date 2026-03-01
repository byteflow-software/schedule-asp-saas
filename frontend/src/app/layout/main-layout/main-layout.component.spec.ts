import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { MainLayoutComponent } from './main-layout.component';
import { TenantService } from '../../core/services/tenant.service';
import { AuthService } from '../../core/services/auth.service';

describe('MainLayoutComponent', () => {
  let component: MainLayoutComponent;
  let fixture: ComponentFixture<MainLayoutComponent>;
  let tenantServiceSpy: { getMyTenant: jest.Mock };
  let dialogSpy: { open: jest.Mock };

  beforeEach(async () => {
    tenantServiceSpy = { getMyTenant: jest.fn() };
    dialogSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [MainLayoutComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: TenantService, useValue: tenantServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        {
          provide: AuthService,
          useValue: {
            currentUser: signal({ fullName: 'John Doe', role: 'Admin' }),
            isAdmin: signal(true),
            isAuthenticated: signal(true),
            logout: jest.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayoutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should open Asaas dialog when tenant has no integration', () => {
      tenantServiceSpy.getMyTenant.mockReturnValue(of({ hasAsaasIntegration: false }));
      component.ngOnInit();
      expect(dialogSpy.open).toHaveBeenCalled();
    });

    it('should not open dialog when tenant has Asaas integration', () => {
      tenantServiceSpy.getMyTenant.mockReturnValue(of({ hasAsaasIntegration: true }));
      component.ngOnInit();
      expect(dialogSpy.open).not.toHaveBeenCalled();
    });
  });
});
