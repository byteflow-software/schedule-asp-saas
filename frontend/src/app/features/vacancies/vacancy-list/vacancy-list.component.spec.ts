import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { VacancyListComponent } from './vacancy-list.component';
import { VacancyService } from '../../../core/services/vacancy.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';

describe('VacancyListComponent', () => {
  let component: VacancyListComponent;
  let fixture: ComponentFixture<VacancyListComponent>;
  let vacancyServiceSpy: { getAll: jest.Mock; delete: jest.Mock };
  let userServiceSpy: { getAll: jest.Mock };
  let serviceServiceSpy: { getAll: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockUsers = [
    { id: 'u1', fullName: 'Dr. Test', isActive: true },
    { id: 'u2', fullName: 'Inactive', isActive: false },
  ];
  const mockServices = [
    { id: 's1', name: 'Haircut', isActive: true },
    { id: 's2', name: 'Old Service', isActive: false },
  ];
  const mockVacancies = [
    { id: 'v1', userName: 'Dr. Test', serviceName: 'Haircut', startTime: '2026-03-04T09:00:00Z', endTime: '2026-03-04T09:30:00Z', isBooked: false },
    { id: 'v2', userName: 'Dr. Test', serviceName: 'Haircut', startTime: '2026-03-04T10:00:00Z', endTime: '2026-03-04T10:30:00Z', isBooked: true },
  ];

  beforeEach(async () => {
    vacancyServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockVacancies)),
      delete: jest.fn().mockReturnValue(of(void 0)),
    };
    userServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockUsers)) };
    serviceServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockServices)) };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [VacancyListComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: VacancyService, useValue: vacancyServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ServiceService, useValue: serviceServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(VacancyListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have correct columns', () => {
    expect(component.columns).toEqual(['userName', 'serviceName', 'startTime', 'endTime', 'isBooked', 'actions']);
  });

  describe('ngOnInit', () => {
    it('should set default date filters', () => {
      component.ngOnInit();
      expect(component.fromDate).toBeTruthy();
      expect(component.toDate).toBeTruthy();
    });

    it('should load and filter active users', () => {
      component.ngOnInit();
      expect(userServiceSpy.getAll).toHaveBeenCalled();
      expect(component.users().length).toBe(1);
      expect(component.users()[0].id).toBe('u1');
    });

    it('should load and filter active services', () => {
      component.ngOnInit();
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
      expect(component.services().length).toBe(1);
    });

    it('should load vacancies', () => {
      component.ngOnInit();
      expect(vacancyServiceSpy.getAll).toHaveBeenCalled();
      expect(component.vacancies().length).toBe(2);
      expect(component.loading()).toBe(false);
    });
  });

  describe('load', () => {
    it('should pass filters to service', () => {
      component.filterUserId = 'u1';
      component.filterServiceId = 's1';
      component.fromDate = '2026-03-01';
      component.toDate = '2026-03-07';
      component.load();
      expect(vacancyServiceSpy.getAll).toHaveBeenCalledWith(
        expect.objectContaining({ userId: 'u1', serviceId: 's1' }),
      );
    });

    it('should not pass empty filters', () => {
      component.filterUserId = '';
      component.filterServiceId = '';
      component.fromDate = '';
      component.toDate = '';
      component.load();
      const callArg = vacancyServiceSpy.getAll.mock.calls[0][0];
      expect(callArg['userId']).toBeUndefined();
      expect(callArg['serviceId']).toBeUndefined();
    });

    it('should handle load error', () => {
      vacancyServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
    });
  });

  describe('openForm', () => {
    it('should open form dialog and reload on close', () => {
      component.ngOnInit();
      vacancyServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(vacancyServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      vacancyServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(vacancyServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('confirmDelete', () => {
    const vacancy = { id: 'v1', userName: 'Dr. Test', startTime: '2026-03-04T09:00:00Z' } as any;

    it('should delete vacancy on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.confirmDelete(vacancy);

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(vacancyServiceSpy.delete).toHaveBeenCalledWith('v1');
      expect(snackBarSpy.open).toHaveBeenCalledWith(expect.stringContaining('excluída'), 'OK', expect.any(Object));
    });

    it('should not delete when dialog cancelled', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });

      component.confirmDelete(vacancy);

      expect(vacancyServiceSpy.delete).not.toHaveBeenCalled();
    });
  });
});
