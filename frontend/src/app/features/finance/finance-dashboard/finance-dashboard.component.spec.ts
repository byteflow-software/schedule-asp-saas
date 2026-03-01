import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { FinanceDashboardComponent, PaymentMethodDialogComponent } from './finance-dashboard.component';
import { TransactionService } from '../../../core/services/transaction.service';

describe('FinanceDashboardComponent', () => {
  let component: FinanceDashboardComponent;
  let fixture: ComponentFixture<FinanceDashboardComponent>;
  let transactionServiceSpy: { getAll: jest.Mock; getSummary: jest.Mock; pay: jest.Mock; cancel: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockSummary = { totalRevenueCents: 50000, totalPendingCents: 10000, totalPaidCents: 40000, count: 10 };
  const mockTransactions = [
    { id: 't1', referenceNumber: 'TX001', customerName: 'John', description: 'Haircut', amountInCents: 5000, status: 'Pending', createdAt: '2026-03-01', invoiceUrl: 'https://pay.test.com' },
    { id: 't2', referenceNumber: 'TX002', customerName: 'Jane', description: 'Massage', amountInCents: 10000, status: 'Paid', createdAt: '2026-03-01', invoiceUrl: null },
  ];
  const mockPaginatedResult = {
    items: mockTransactions, totalCount: 2, totalPages: 1, hasPreviousPage: false, hasNextPage: false,
  };

  beforeEach(async () => {
    transactionServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockPaginatedResult)),
      getSummary: jest.fn().mockReturnValue(of(mockSummary)),
      pay: jest.fn().mockReturnValue(of(void 0)),
      cancel: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of('pix') })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [FinanceDashboardComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: TransactionService, useValue: transactionServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(FinanceDashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with loading true', () => {
    expect(component.loading()).toBe(true);
  });

  describe('ngOnInit', () => {
    it('should set default date filters', () => {
      component.ngOnInit();
      expect(component.fromDate).toBeTruthy();
      expect(component.toDate).toBeTruthy();
    });

    it('should load transactions and summary', () => {
      component.ngOnInit();
      expect(transactionServiceSpy.getAll).toHaveBeenCalled();
      expect(transactionServiceSpy.getSummary).toHaveBeenCalled();
      expect(component.transactions().length).toBe(2);
      expect(component.summary().totalRevenueCents).toBe(50000);
      expect(component.loading()).toBe(false);
    });

    it('should set pagination signals', () => {
      component.ngOnInit();
      expect(component.totalPages()).toBe(1);
      expect(component.totalCount()).toBe(2);
    });
  });

  describe('load', () => {
    it('should handle getAll error', () => {
      transactionServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
    });

    it('should handle getSummary error gracefully', () => {
      transactionServiceSpy.getSummary.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      // Should not crash
      expect(component.loading()).toBe(false);
    });

    it('should pass filters', () => {
      component.fromDate = '2026-03-01';
      component.toDate = '2026-03-31';
      component.statusFilter = 'Pending';
      component.load();
      expect(transactionServiceSpy.getAll).toHaveBeenCalledWith(
        expect.objectContaining({ from: '2026-03-01T00:00:00Z', to: '2026-03-31T23:59:59Z', status: 'Pending' }),
      );
    });

    it('should pass undefined for empty filters', () => {
      component.fromDate = '';
      component.toDate = '';
      component.statusFilter = '';
      component.load();
      expect(transactionServiceSpy.getAll).toHaveBeenCalledWith(
        expect.objectContaining({ from: undefined, to: undefined, status: undefined }),
      );
    });
  });

  describe('loadPage', () => {
    it('should update page and reload', () => {
      component.ngOnInit();
      transactionServiceSpy.getAll.mockClear();
      component.loadPage(3);
      expect(component.pageNumber).toBe(3);
      expect(transactionServiceSpy.getAll).toHaveBeenCalled();
    });
  });

  describe('openPaymentDialog', () => {
    const transaction = { id: 't1', status: 'Pending' } as any;

    it('should open dialog and call pay on selection', () => {
      // Access the injected dialog and spy on it
      const dialog = (component as any).dialog as MatDialog;
      const openSpy = jest.spyOn(dialog, 'open').mockReturnValue({ afterClosed: () => of('pix') } as any);

      component.openPaymentDialog(transaction);

      expect(openSpy).toHaveBeenCalled();
      expect(transactionServiceSpy.pay).toHaveBeenCalledWith('t1', 'pix');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Pagamento registrado com sucesso.', 'OK', expect.any(Object));
    });

    it('should not call pay if dialog cancelled', () => {
      const dialog = (component as any).dialog as MatDialog;
      jest.spyOn(dialog, 'open').mockReturnValue({ afterClosed: () => of(null) } as any);

      component.openPaymentDialog(transaction);

      expect(transactionServiceSpy.pay).not.toHaveBeenCalled();
    });

    it('should handle pay error', () => {
      const dialog = (component as any).dialog as MatDialog;
      jest.spyOn(dialog, 'open').mockReturnValue({ afterClosed: () => of('cartao') } as any);
      transactionServiceSpy.pay.mockReturnValue(throwError(() => new Error('fail')));

      component.openPaymentDialog(transaction);

      expect(snackBarSpy.open).toHaveBeenCalledWith('Erro ao registrar pagamento.', 'OK', expect.any(Object));
    });
  });

  describe('cancelTransaction', () => {
    const transaction = { id: 't1' } as any;

    it('should cancel and show success', () => {
      component.cancelTransaction(transaction);
      expect(transactionServiceSpy.cancel).toHaveBeenCalledWith('t1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Transação cancelada.', 'OK', expect.any(Object));
    });

    it('should handle cancel error', () => {
      transactionServiceSpy.cancel.mockReturnValue(throwError(() => new Error('fail')));
      component.cancelTransaction(transaction);
      expect(snackBarSpy.open).toHaveBeenCalledWith('Erro ao cancelar transação.', 'OK', expect.any(Object));
    });
  });
});

describe('PaymentMethodDialogComponent (finance)', () => {
  let component: PaymentMethodDialogComponent;
  let dialogRefSpy: { close: jest.Mock };

  beforeEach(() => {
    dialogRefSpy = { close: jest.fn() };
    TestBed.configureTestingModule({
      imports: [PaymentMethodDialogComponent],
      providers: [{ provide: MatDialogRef, useValue: dialogRefSpy }],
    });
    const fixture = TestBed.createComponent(PaymentMethodDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should close dialog with method on select', () => {
    component.select('pix');
    expect(dialogRefSpy.close).toHaveBeenCalledWith('pix');
  });
});
