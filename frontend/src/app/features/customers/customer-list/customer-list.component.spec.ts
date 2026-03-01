import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { CustomerListComponent } from './customer-list.component';
import { CustomerService } from '../../../core/services/customer.service';

describe('CustomerListComponent', () => {
  let component: CustomerListComponent;
  let fixture: ComponentFixture<CustomerListComponent>;
  let customerServiceSpy: { getAll: jest.Mock; delete: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockPaginatedResult = {
    items: [
      { id: '1', fullName: 'John Doe', cpfCnpj: '123.456.789-00', email: 'john@example.com', phone: '11999999999', createdAt: '2026-02-28T10:00:00Z' },
    ],
    totalPages: 2, totalCount: 15, hasPreviousPage: false, hasNextPage: true,
  };

  beforeEach(async () => {
    customerServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockPaginatedResult)),
      delete: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [CustomerListComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: CustomerService, useValue: customerServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have correct table columns', () => {
    expect(component.columns).toEqual(['fullName', 'cpfCnpj', 'email', 'phone', 'createdAt', 'actions']);
  });

  it('should start with empty search', () => {
    expect(component.search).toBe('');
  });

  describe('ngOnInit', () => {
    it('should load customers', () => {
      component.ngOnInit();
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
      expect(component.customers().length).toBe(1);
      expect(component.loading()).toBe(false);
    });

    it('should set pagination signals', () => {
      component.ngOnInit();
      expect(component.totalPages()).toBe(2);
      expect(component.totalCount()).toBe(15);
      expect(component.hasPreviousPage()).toBe(false);
      expect(component.hasNextPage()).toBe(true);
    });

    it('should handle load error', () => {
      customerServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.ngOnInit();
      expect(component.loading()).toBe(false);
    });
  });

  describe('loadPage', () => {
    it('should update page and reload', () => {
      component.ngOnInit();
      customerServiceSpy.getAll.mockClear();
      component.loadPage(2);
      expect(component.pageNumber).toBe(2);
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
    });
  });

  describe('onSearch', () => {
    it('should debounce search and reset to page 1', fakeAsync(() => {
      component.ngOnInit();
      customerServiceSpy.getAll.mockClear();
      component.search = 'John';
      component.onSearch();
      tick(400);
      expect(component.pageNumber).toBe(1);
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
    }));

    it('should clear previous timeout on rapid input', fakeAsync(() => {
      component.ngOnInit();
      customerServiceSpy.getAll.mockClear();
      component.search = 'J';
      component.onSearch();
      tick(200);
      component.search = 'Jo';
      component.onSearch();
      tick(400);
      // Only called once (second debounce wins)
      expect(customerServiceSpy.getAll).toHaveBeenCalledTimes(1);
    }));
  });

  describe('openForm', () => {
    it('should open form dialog for new customer and reload on close', () => {
      component.ngOnInit();
      customerServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(customerServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should open form dialog for existing customer', () => {
      const customer = { id: '1', fullName: 'John', cpfCnpj: '123', email: 'j@t.com', phone: '', createdAt: '' } as any;
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });
      component.openForm(customer);
      expect(dialogSpy.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: customer }));
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      customerServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(customerServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('confirmDelete', () => {
    const customer = { id: '1', fullName: 'John Doe', cpfCnpj: '123', email: 'j@t.com', phone: '', createdAt: '' } as any;

    it('should delete customer on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.confirmDelete(customer);

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(customerServiceSpy.delete).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Cliente excluído com sucesso.', 'OK', expect.any(Object));
    });

    it('should not delete when dialog cancelled', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });

      component.confirmDelete(customer);

      expect(customerServiceSpy.delete).not.toHaveBeenCalled();
    });
  });
});
