import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { TransactionService } from './transaction.service';

describe('TransactionService', () => {
  let service: TransactionService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TransactionService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(TransactionService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET transactions without filters', () => {
      service.getAll().subscribe((result) => {
        expect(result.items.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/transactions');
      expect(req.request.method).toBe('GET');
      req.flush({ items: [{ id: '1' }], totalCount: 1 });
    });

    it('should GET transactions with filters', () => {
      service
        .getAll({
          from: '2026-01-01',
          to: '2026-01-31',
          status: 'Paid',
          customerId: 'c1',
          pageNumber: 1,
          pageSize: 10,
        })
        .subscribe((result) => {
          expect(result).toBeTruthy();
        });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/transactions' &&
          r.params.get('from') === '2026-01-01' &&
          r.params.get('to') === '2026-01-31' &&
          r.params.get('status') === 'Paid' &&
          r.params.get('customerId') === 'c1'
      );
      expect(req.request.method).toBe('GET');
      req.flush({ items: [], totalCount: 0 });
    });
  });

  describe('getById', () => {
    it('should GET transaction by id', () => {
      service.getById('t1').subscribe((result) => {
        expect(result.id).toBe('t1');
      });

      const req = httpMock.expectOne('/api/transactions/t1');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 't1' });
    });
  });

  describe('getSummary', () => {
    it('should GET transaction summary without date range', () => {
      service.getSummary().subscribe((result) => {
        expect(result.count).toBe(5);
      });

      const req = httpMock.expectOne('/api/transactions/summary');
      expect(req.request.method).toBe('GET');
      req.flush({
        totalRevenueCents: 10000,
        totalPendingCents: 3000,
        totalPaidCents: 7000,
        count: 5,
      });
    });

    it('should GET transaction summary with date range', () => {
      service.getSummary('2026-01-01', '2026-01-31').subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/transactions/summary' &&
          r.params.get('from') === '2026-01-01' &&
          r.params.get('to') === '2026-01-31'
      );
      expect(req.request.method).toBe('GET');
      req.flush({
        totalRevenueCents: 0,
        totalPendingCents: 0,
        totalPaidCents: 0,
        count: 0,
      });
    });
  });

  describe('pay', () => {
    it('should PATCH to pay a transaction', () => {
      service.pay('t1', 'PIX').subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/transactions/t1/pay');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({ paymentMethod: 'PIX' });
      req.flush({ id: 't1', status: 'Paid' });
    });
  });

  describe('cancel', () => {
    it('should PATCH to cancel a transaction', () => {
      service.cancel('t1').subscribe();

      const req = httpMock.expectOne('/api/transactions/t1/cancel');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });
});
