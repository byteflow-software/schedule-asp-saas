import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { AppointmentService } from '../../core/services/appointment.service';
import { CustomerService } from '../../core/services/customer.service';
import { TenantService } from '../../core/services/tenant.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let appointmentServiceSpy: { getAll: jest.Mock };
  let customerServiceSpy: { getAll: jest.Mock };
  let tenantServiceSpy: { getMyTenant: jest.Mock };

  const mockTodayResult = {
    items: [
      { id: 'a1', startTime: '2026-03-01T10:00:00Z', endTime: '2026-03-01T11:00:00Z', customerName: 'John', userName: 'Dr', status: 'Confirmed' },
      { id: 'a2', startTime: '2026-03-01T14:00:00Z', endTime: '2026-03-01T15:00:00Z', customerName: 'Jane', userName: 'Dr', status: 'PendingPayment' },
    ],
    totalCount: 2, totalPages: 1, hasPreviousPage: false, hasNextPage: false,
  };

  const mockWeekResult = {
    items: [], totalCount: 5, totalPages: 1, hasPreviousPage: false, hasNextPage: false,
  };

  const mockCustomerResult = {
    items: [], totalCount: 42, totalPages: 1, hasPreviousPage: false, hasNextPage: false,
  };

  const mockTenant = { id: '1', name: 'Test Tenant', hasAsaasIntegration: true };

  beforeEach(async () => {
    appointmentServiceSpy = {
      getAll: jest.fn().mockImplementation((params: any) => {
        if (params.pageSize === 50) return of(mockTodayResult);
        if (params.pageSize === 1) return of(mockWeekResult);
        return of({ items: [], totalCount: 0 });
      }),
    };
    customerServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockCustomerResult)) };
    tenantServiceSpy = { getMyTenant: jest.fn().mockReturnValue(of(mockTenant)) };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: CustomerService, useValue: customerServiceSpy },
        { provide: TenantService, useValue: tenantServiceSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with loading true', () => {
    expect(component.loading()).toBe(true);
  });

  describe('ngOnInit', () => {
    it('should load today appointments', () => {
      component.ngOnInit();
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
      expect(component.todayAppointments().length).toBe(2);
    });

    it('should load week appointments count', () => {
      component.ngOnInit();
      expect(component.weekAppointments()).toBe(5);
    });

    it('should load total customers', () => {
      component.ngOnInit();
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
      expect(component.totalCustomers()).toBe(42);
    });

    it('should load tenant and set loading to false', () => {
      component.ngOnInit();
      expect(tenantServiceSpy.getMyTenant).toHaveBeenCalled();
      expect(component.tenant()).toEqual(mockTenant);
      expect(component.loading()).toBe(false);
    });

    it('should handle appointment load error gracefully', () => {
      appointmentServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      // Should not crash, errors are caught
      expect(component.todayAppointments().length).toBe(0);
    });

    it('should handle customer load error gracefully', () => {
      customerServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.totalCustomers()).toBe(0);
    });

    it('should handle tenant load error and set loading false', () => {
      tenantServiceSpy.getMyTenant.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.loading()).toBe(false);
    });
  });
});
