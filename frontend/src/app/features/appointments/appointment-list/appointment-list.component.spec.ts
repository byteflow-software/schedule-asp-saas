import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { AppointmentListComponent } from './appointment-list.component';
import { AppointmentService } from '../../../core/services/appointment.service';

describe('AppointmentListComponent', () => {
  let component: AppointmentListComponent;
  let fixture: ComponentFixture<AppointmentListComponent>;
  let appointmentServiceSpy: { getAll: jest.Mock; cancel: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };
  let router: Router;

  const mockPaginatedResult = {
    items: [
      { id: '1', startTime: '2026-03-01T10:00:00Z', endTime: '2026-03-01T11:00:00Z', customerName: 'John Doe', serviceName: 'Haircut', userName: 'Barber', priceInCents: 5000, status: 'Confirmed' },
      { id: '2', startTime: '2026-03-01T14:00:00Z', endTime: '2026-03-01T15:00:00Z', customerName: 'Jane', serviceName: 'Massage', userName: 'Therapist', priceInCents: 10000, status: 'PendingPayment' },
    ],
    totalPages: 1, totalCount: 2, hasPreviousPage: false, hasNextPage: false,
  };

  beforeEach(async () => {
    appointmentServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockPaginatedResult)),
      cancel: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [AppointmentListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(AppointmentListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have correct table columns', () => {
    expect(component.columns).toEqual(['startTime', 'customerName', 'serviceName', 'userName', 'priceInCents', 'status', 'actions']);
  });

  describe('ngOnInit', () => {
    it('should set date filters and load appointments', () => {
      component.ngOnInit();
      expect(component.fromDate).toBeTruthy();
      expect(component.toDate).toBeTruthy();
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
      expect(component.appointments().length).toBe(2);
      expect(component.loading()).toBe(false);
    });

    it('should set pagination signals', () => {
      component.ngOnInit();
      expect(component.totalPages()).toBe(1);
      expect(component.totalCount()).toBe(2);
      expect(component.hasPreviousPage()).toBe(false);
      expect(component.hasNextPage()).toBe(false);
    });
  });

  describe('load', () => {
    it('should filter by status when statusFilter is set', () => {
      component.fromDate = '2026-03-01';
      component.toDate = '2026-03-31';
      component.statusFilter = 'Confirmed';
      component.load();
      expect(component.appointments().length).toBe(1);
      expect(component.appointments()[0].status).toBe('Confirmed');
    });

    it('should handle error gracefully', () => {
      appointmentServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
    });
  });

  describe('loadPage', () => {
    it('should update page and reload', () => {
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();
      component.loadPage(2);
      expect(component.pageNumber).toBe(2);
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
    });
  });

  describe('openForm', () => {
    it('should open dialog and reload on close with result', () => {
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(appointmentServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('viewDetail', () => {
    it('should navigate to appointment detail', () => {
      component.viewDetail({ id: 'abc' } as any);
      expect(router.navigate).toHaveBeenCalledWith(['/appointments', 'abc']);
    });
  });

  describe('confirmCancel', () => {
    const apt = { id: '1', customerName: 'John' } as any;

    it('should cancel and show success on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();

      component.confirmCancel(apt);

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(appointmentServiceSpy.cancel).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Agendamento cancelado.', 'OK', expect.any(Object));
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not cancel when dialog is dismissed', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });

      component.confirmCancel(apt);

      expect(appointmentServiceSpy.cancel).not.toHaveBeenCalled();
    });
  });
});
