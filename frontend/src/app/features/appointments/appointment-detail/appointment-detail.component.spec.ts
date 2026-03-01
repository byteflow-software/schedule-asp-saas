import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter, ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { AppointmentDetailComponent, PaymentMethodDialogComponent } from './appointment-detail.component';
import { AppointmentService } from '../../../core/services/appointment.service';
import { TransactionService } from '../../../core/services/transaction.service';
import { MatDialogRef } from '@angular/material/dialog';

describe('AppointmentDetailComponent', () => {
  let component: AppointmentDetailComponent;
  let fixture: ComponentFixture<AppointmentDetailComponent>;
  let appointmentServiceSpy: jest.Mocked<Partial<AppointmentService>>;
  let transactionServiceSpy: jest.Mocked<Partial<TransactionService>>;
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };
  let routerSpy: { navigate: jest.Mock };

  const mockAppointment = {
    id: '1',
    startTime: '2026-03-01T10:00:00Z',
    endTime: '2026-03-01T11:00:00Z',
    customerName: 'John Doe',
    serviceName: 'Haircut',
    userName: 'Barber',
    priceInCents: 5000,
    status: 'Confirmed',
    notes: 'Test notes',
    createdAt: '2026-02-28T10:00:00Z',
    transaction: null,
  };

  const mockAppointmentWithTransaction = {
    ...mockAppointment,
    status: 'PendingPayment',
    transaction: { id: 'tx1', referenceNumber: 'REF-001', amountInCents: 5000, status: 'Pending', invoiceUrl: 'https://pay.test.com' },
  };

  beforeEach(async () => {
    appointmentServiceSpy = {
      getById: jest.fn().mockReturnValue(of(mockAppointment)),
      cancel: jest.fn().mockReturnValue(of(void 0)),
      markDone: jest.fn().mockReturnValue(of(void 0)),
    };
    transactionServiceSpy = {
      pay: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };
    routerSpy = { navigate: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [AppointmentDetailComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: TransactionService, useValue: transactionServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => '1' } } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentDetailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with loading true', () => {
    expect(component.loading()).toBe(true);
  });

  it('should load appointment on init', () => {
    component.ngOnInit();
    expect(appointmentServiceSpy.getById).toHaveBeenCalledWith('1');
    expect(component.appointment()).toEqual(mockAppointment);
    expect(component.loading()).toBe(false);
  });

  it('should not load if no id in route', () => {
    TestBed.resetTestingModule();
    // Re-setup with null id
    TestBed.configureTestingModule({
      imports: [AppointmentDetailComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: TransactionService, useValue: transactionServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } },
      ],
    });
    const fix = TestBed.createComponent(AppointmentDetailComponent);
    const comp = fix.componentInstance;
    comp.ngOnInit();
    expect(appointmentServiceSpy.getById).not.toHaveBeenCalled();
  });

  it('should navigate to /appointments on load error', () => {
    appointmentServiceSpy.getById!.mockReturnValue(throwError(() => new Error('not found')));
    component.ngOnInit();
    expect(component.loading()).toBe(false);
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/appointments']);
  });

  it('should navigate back on goBack', () => {
    component.goBack();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/appointments']);
  });

  describe('confirmPayment', () => {
    it('should return early if no transaction', () => {
      component.appointment.set(mockAppointment as any);
      component.confirmPayment();
      expect(dialogSpy.open).not.toHaveBeenCalled();
    });

    it('should open payment dialog and call pay on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of('pix') });
      component.appointment.set(mockAppointmentWithTransaction as any);

      component.confirmPayment();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(transactionServiceSpy.pay).toHaveBeenCalledWith('tx1', 'pix');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Pagamento registrado!', 'OK', expect.any(Object));
    });

    it('should not call pay if dialog returns falsy', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });
      component.appointment.set(mockAppointmentWithTransaction as any);

      component.confirmPayment();

      expect(transactionServiceSpy.pay).not.toHaveBeenCalled();
    });
  });

  describe('markDone', () => {
    it('should return early if no appointment', () => {
      component.appointment.set(null);
      component.markDone();
      expect(appointmentServiceSpy.markDone).not.toHaveBeenCalled();
    });

    it('should call markDone and reload on success', () => {
      component.appointment.set(mockAppointment as any);
      component.markDone();
      expect(appointmentServiceSpy.markDone).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Agendamento concluído!', 'OK', expect.any(Object));
    });
  });

  describe('cancelAppointment', () => {
    it('should return early if no appointment', () => {
      component.appointment.set(null);
      component.cancelAppointment();
      expect(dialogSpy.open).not.toHaveBeenCalled();
    });

    it('should open confirm dialog and cancel on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });
      component.appointment.set(mockAppointment as any);

      component.cancelAppointment();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(appointmentServiceSpy.cancel).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Agendamento cancelado.', 'OK', expect.any(Object));
    });

    it('should not cancel if dialog returns false', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });
      component.appointment.set(mockAppointment as any);

      component.cancelAppointment();

      expect(appointmentServiceSpy.cancel).not.toHaveBeenCalled();
    });
  });
});

describe('PaymentMethodDialogComponent (detail)', () => {
  let component: PaymentMethodDialogComponent;
  let dialogRefSpy: { close: jest.Mock };

  beforeEach(() => {
    dialogRefSpy = { close: jest.fn() };
    TestBed.configureTestingModule({
      imports: [PaymentMethodDialogComponent, NoopAnimationsModule],
      providers: [{ provide: MatDialogRef, useValue: dialogRefSpy }],
    });
    const fixture = TestBed.createComponent(PaymentMethodDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should close dialog with method on select', () => {
    component.select('dinheiro');
    expect(dialogRefSpy.close).toHaveBeenCalledWith('dinheiro');
  });
});
