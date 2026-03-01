import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { ServiceListComponent } from './service-list.component';
import { ServiceService } from '../../../core/services/service.service';

describe('ServiceListComponent', () => {
  let component: ServiceListComponent;
  let fixture: ComponentFixture<ServiceListComponent>;
  let serviceServiceSpy: { getAll: jest.Mock; delete: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockServices = [
    { id: '1', name: 'Haircut', description: 'Basic', durationMinutes: 30, priceInCents: 5000, isActive: true },
  ];

  beforeEach(async () => {
    serviceServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockServices)),
      delete: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [ServiceListComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ServiceService, useValue: serviceServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ServiceListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have correct table columns', () => {
    expect(component.columns).toEqual(['name', 'durationMinutes', 'priceInCents', 'isActive', 'actions']);
  });

  it('should start with loading true', () => {
    expect(component.loading()).toBe(true);
  });

  describe('ngOnInit', () => {
    it('should load services', () => {
      component.ngOnInit();
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
      expect(component.services().length).toBe(1);
      expect(component.loading()).toBe(false);
    });
  });

  describe('load', () => {
    it('should handle error gracefully', () => {
      serviceServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
    });
  });

  describe('openForm', () => {
    it('should open form and reload on close with result', () => {
      component.ngOnInit();
      serviceServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should open form with existing service data', () => {
      component.ngOnInit();
      serviceServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm(mockServices[0] as any);

      expect(dialogSpy.open).toHaveBeenCalledWith(expect.any(Function), expect.objectContaining({ data: mockServices[0] }));
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      serviceServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(serviceServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('confirmDelete', () => {
    const service = { id: '1', name: 'Haircut' } as any;

    it('should delete and show success on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });
      component.ngOnInit();
      serviceServiceSpy.getAll.mockClear();

      component.confirmDelete(service);

      expect(serviceServiceSpy.delete).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Serviço excluído com sucesso.', 'OK', expect.any(Object));
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not delete when dialog is dismissed', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });

      component.confirmDelete(service);

      expect(serviceServiceSpy.delete).not.toHaveBeenCalled();
    });
  });
});
