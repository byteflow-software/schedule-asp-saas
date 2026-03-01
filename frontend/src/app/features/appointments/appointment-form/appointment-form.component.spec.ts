import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Clipboard } from '@angular/cdk/clipboard';
import { of, throwError } from 'rxjs';
import { AppointmentFormComponent } from './appointment-form.component';
import { AppointmentService } from '../../../core/services/appointment.service';
import { CustomerService } from '../../../core/services/customer.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';
import { VacancyService } from '../../../core/services/vacancy.service';
import { AuthService } from '../../../core/services/auth.service';

describe('AppointmentFormComponent', () => {
  let component: AppointmentFormComponent;
  let fixture: ComponentFixture<AppointmentFormComponent>;
  let dialogRefSpy: { close: jest.Mock };
  let snackBarSpy: { open: jest.Mock };
  let clipboardSpy: { copy: jest.Mock };
  let appointmentServiceSpy: { create: jest.Mock };
  let customerServiceSpy: { getAll: jest.Mock; create: jest.Mock };
  let userServiceSpy: { getAll: jest.Mock };
  let serviceServiceSpy: { getAll: jest.Mock };
  let vacancyServiceSpy: { getAll: jest.Mock };
  let authServiceSpy: { currentUser: jest.Mock };

  const mockUsers = [
    { id: 'u1', fullName: 'Dr. Test', email: 'dr@test.com', role: 'Admin', isActive: true, createdAt: '' },
    { id: 'u2', fullName: 'Inactive Doc', email: 'x@test.com', role: 'User', isActive: false, createdAt: '' },
  ];

  const mockServices = [
    { id: 's1', name: 'Haircut', durationMinutes: 30, priceInCents: 5000, isActive: true, description: 'Desc' },
    { id: 's2', name: 'Inactive', durationMinutes: 60, priceInCents: 10000, isActive: false, description: '' },
  ];

  const mockCustomers = {
    items: [{ id: 'c1', fullName: 'John Doe', email: 'john@test.com', cpfCnpj: '123', phone: '' }],
    totalCount: 1, totalPages: 1, hasPreviousPage: false, hasNextPage: false,
  };

  beforeEach(async () => {
    dialogRefSpy = { close: jest.fn() };
    snackBarSpy = { open: jest.fn() };
    clipboardSpy = { copy: jest.fn() };
    appointmentServiceSpy = { create: jest.fn().mockReturnValue(of({})) };
    customerServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockCustomers)),
      create: jest.fn().mockReturnValue(of({ id: 'c2', fullName: 'New Customer', email: 'new@test.com' })),
    };
    userServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockUsers)) };
    serviceServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockServices)) };
    vacancyServiceSpy = { getAll: jest.fn().mockReturnValue(of([])) };
    authServiceSpy = { currentUser: jest.fn().mockReturnValue(null) };

    await TestBed.configureTestingModule({
      imports: [AppointmentFormComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: Clipboard, useValue: clipboardSpy },
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: CustomerService, useValue: customerServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ServiceService, useValue: serviceServiceSpy },
        { provide: VacancyService, useValue: vacancyServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have stepper form groups', () => {
    expect(component.clientForm).toBeDefined();
    expect(component.serviceForm).toBeDefined();
    expect(component.timeForm).toBeDefined();
  });

  it('should start with createdAppointment as null', () => {
    expect(component.createdAppointment()).toBeNull();
  });

  describe('ngOnInit', () => {
    it('should load customers', () => {
      component.ngOnInit();
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
      expect(component.customers.length).toBe(1);
    });

    it('should load and filter active users', () => {
      component.ngOnInit();
      expect(userServiceSpy.getAll).toHaveBeenCalled();
      expect(component.users.length).toBe(1);
      expect(component.users[0].id).toBe('u1');
    });

    it('should load and filter active services', () => {
      component.ngOnInit();
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
      expect(component.services.length).toBe(1);
    });

    it('should pre-select current user if found in users list', () => {
      authServiceSpy.currentUser.mockReturnValue({ userId: 'u1', fullName: 'Dr. Test', role: 'Admin' });
      component.ngOnInit();
      expect(component.serviceForm.getRawValue().userId).toBe('u1');
    });

    it('should not pre-select if current user not in users list', () => {
      authServiceSpy.currentUser.mockReturnValue({ userId: 'unknown', fullName: 'Unknown', role: 'Admin' });
      component.ngOnInit();
      expect(component.serviceForm.getRawValue().userId).toBe('');
    });

    it('should handle user service error with current user fallback', () => {
      authServiceSpy.currentUser.mockReturnValue({ userId: 'u1', fullName: 'Dr. Test', role: 'Admin' });
      userServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.users.length).toBe(1);
      expect(component.users[0].id).toBe('u1');
      expect(component.serviceForm.getRawValue().userId).toBe('u1');
    });

    it('should handle user service error without current user', () => {
      authServiceSpy.currentUser.mockReturnValue(null);
      userServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.users.length).toBe(0);
    });
  });

  describe('createCustomer', () => {
    it('should return early if required fields are missing', () => {
      component.clientForm.patchValue({ newCustomerName: '', newCustomerEmail: '', newCustomerCpfCnpj: '' });
      component.createCustomer();
      expect(customerServiceSpy.create).not.toHaveBeenCalled();
    });

    it('should create customer and update form on success', () => {
      component.clientForm.patchValue({
        newCustomerName: 'Jane',
        newCustomerEmail: 'jane@test.com',
        newCustomerCpfCnpj: '98765432100',
        newCustomerPhone: '11888888888',
      });
      component.showNewCustomer = true;

      component.createCustomer();

      expect(customerServiceSpy.create).toHaveBeenCalled();
      expect(component.clientForm.getRawValue().customerId).toBe('c2');
      expect(component.showNewCustomer).toBe(false);
      expect(component.creatingCustomer).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith('Cliente cadastrado!', 'OK', expect.any(Object));
    });

    it('should handle create customer error', () => {
      customerServiceSpy.create.mockReturnValue(throwError(() => new Error('fail')));
      component.clientForm.patchValue({
        newCustomerName: 'Jane',
        newCustomerEmail: 'jane@test.com',
        newCustomerCpfCnpj: '98765432100',
      });
      component.createCustomer();
      expect(component.creatingCustomer).toBe(false);
    });

    it('should send phone as undefined when empty', () => {
      component.clientForm.patchValue({
        newCustomerName: 'Jane',
        newCustomerEmail: 'jane@test.com',
        newCustomerCpfCnpj: '123',
        newCustomerPhone: '',
      });
      component.createCustomer();
      expect(customerServiceSpy.create).toHaveBeenCalledWith(
        expect.objectContaining({ phone: undefined }),
      );
    });
  });

  describe('selectService', () => {
    it('should set selectedService and update form', () => {
      const service = mockServices[0] as any;
      component.selectService(service);
      expect(component.selectedService).toBe(service);
      expect(component.serviceForm.getRawValue().serviceId).toBe('s1');
    });
  });

  describe('loadVacancies', () => {
    it('should load vacancies when userId and serviceId are set', () => {
      component.serviceForm.patchValue({ userId: 'u1', serviceId: 's1' });
      component.loadVacancies();
      expect(vacancyServiceSpy.getAll).toHaveBeenCalledWith({ userId: 'u1', serviceId: 's1', available: true });
    });

    it('should return early when userId is empty', () => {
      component.serviceForm.patchValue({ userId: '', serviceId: 's1' });
      component.loadVacancies();
      expect(vacancyServiceSpy.getAll).not.toHaveBeenCalled();
    });

    it('should return early when serviceId is empty', () => {
      component.serviceForm.patchValue({ userId: 'u1', serviceId: '' });
      component.loadVacancies();
      expect(vacancyServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('selectVacancy', () => {
    it('should set vacancy and populate time form', () => {
      const vacancy = { id: 'v1', startTime: '2026-03-04T09:00:00Z', endTime: '2026-03-04T09:30:00Z' } as any;
      component.selectVacancy(vacancy);
      expect(component.selectedVacancyId).toBe('v1');
      expect(component.timeForm.getRawValue().startTime).toBeTruthy();
      expect(component.timeForm.getRawValue().endTime).toBeTruthy();
    });
  });

  describe('onStartTimeChange', () => {
    it('should auto-calculate endTime when service and startTime are set', () => {
      component.selectedService = mockServices[0] as any; // 30 min
      component.timeForm.patchValue({ startTime: '2026-03-04T09:00' });
      component.onStartTimeChange();
      expect(component.timeForm.getRawValue().endTime).toBeTruthy();
      expect(component.selectedVacancyId).toBeNull();
    });

    it('should do nothing when no selectedService', () => {
      component.selectedService = null;
      component.timeForm.patchValue({ startTime: '2026-03-04T09:00' });
      component.onStartTimeChange();
      expect(component.timeForm.getRawValue().endTime).toBe('');
    });

    it('should do nothing when startTime is empty', () => {
      component.selectedService = mockServices[0] as any;
      component.timeForm.patchValue({ startTime: '' });
      component.onStartTimeChange();
      // endTime should remain unchanged
    });
  });

  describe('getSelected*Name helpers', () => {
    it('getSelectedCustomerName should return customer name', () => {
      component.customers = mockCustomers.items as any;
      component.clientForm.patchValue({ customerId: 'c1' });
      expect(component.getSelectedCustomerName()).toBe('John Doe');
    });

    it('getSelectedCustomerName should return empty when not found', () => {
      component.customers = [];
      component.clientForm.patchValue({ customerId: 'c1' });
      expect(component.getSelectedCustomerName()).toBe('');
    });

    it('getSelectedServiceName should return service name', () => {
      component.selectedService = mockServices[0] as any;
      expect(component.getSelectedServiceName()).toBe('Haircut');
    });

    it('getSelectedServiceName should return empty when no service', () => {
      component.selectedService = null;
      expect(component.getSelectedServiceName()).toBe('');
    });

    it('getSelectedUserName should return user name', () => {
      component.users = mockUsers.filter(u => u.isActive) as any;
      component.serviceForm.patchValue({ userId: 'u1' });
      expect(component.getSelectedUserName()).toBe('Dr. Test');
    });

    it('getSelectedUserName should return empty when not found', () => {
      component.users = [];
      component.serviceForm.patchValue({ userId: 'u1' });
      expect(component.getSelectedUserName()).toBe('');
    });
  });

  describe('save', () => {
    beforeEach(() => {
      component.clientForm.patchValue({ customerId: 'c1' });
      component.serviceForm.patchValue({ userId: 'u1', serviceId: 's1' });
      component.timeForm.patchValue({ startTime: '2026-03-04T09:00', endTime: '2026-03-04T09:30' });
    });

    it('should call create and set createdAppointment on success', () => {
      const result = { id: 'a1', serviceName: 'Haircut', customerName: 'John', status: 'PendingPayment' };
      appointmentServiceSpy.create.mockReturnValue(of(result));

      component.save();

      expect(appointmentServiceSpy.create).toHaveBeenCalled();
      expect(component.createdAppointment()).toEqual(result);
      expect(component.saving).toBe(false);
    });

    it('should include vacancyId when selected', () => {
      component.selectedVacancyId = 'v1';
      appointmentServiceSpy.create.mockReturnValue(of({ id: 'a1' }));
      component.save();
      expect(appointmentServiceSpy.create).toHaveBeenCalledWith(
        expect.objectContaining({ vacancyId: 'v1' }),
      );
    });

    it('should include notes when provided', () => {
      component.notes = 'Test notes';
      appointmentServiceSpy.create.mockReturnValue(of({ id: 'a1' }));
      component.save();
      expect(appointmentServiceSpy.create).toHaveBeenCalledWith(
        expect.objectContaining({ notes: 'Test notes' }),
      );
    });

    it('should handle save error', () => {
      appointmentServiceSpy.create.mockReturnValue(throwError(() => new Error('fail')));
      component.save();
      expect(component.saving).toBe(false);
      expect(component.createdAppointment()).toBeNull();
    });
  });

  describe('copyPaymentLink', () => {
    it('should copy URL and show snackbar when transaction has invoiceUrl', () => {
      component.createdAppointment.set({
        transaction: { invoiceUrl: 'https://pay.asaas.com/123' },
      } as any);

      component.copyPaymentLink();

      expect(clipboardSpy.copy).toHaveBeenCalledWith('https://pay.asaas.com/123');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Link copiado!', 'OK', expect.any(Object));
    });

    it('should not copy when no transaction url', () => {
      component.createdAppointment.set({ transaction: null } as any);
      component.copyPaymentLink();
      expect(clipboardSpy.copy).not.toHaveBeenCalled();
    });

    it('should not copy when createdAppointment is null', () => {
      component.createdAppointment.set(null);
      component.copyPaymentLink();
      expect(clipboardSpy.copy).not.toHaveBeenCalled();
    });
  });
});
