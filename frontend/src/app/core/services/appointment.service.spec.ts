import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { AppointmentService } from './appointment.service';

describe('AppointmentService', () => {
  let service: AppointmentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AppointmentService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AppointmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET appointments without filters', () => {
      service.getAll().subscribe((result) => {
        expect(result.items.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/appointments');
      expect(req.request.method).toBe('GET');
      req.flush({ items: [{ id: '1' }], totalCount: 1 });
    });

    it('should GET appointments with filters', () => {
      service
        .getAll({
          from: '2026-01-01',
          to: '2026-01-31',
          userId: 'u1',
          customerId: 'c1',
          pageNumber: 2,
          pageSize: 20,
        })
        .subscribe((result) => {
          expect(result).toBeTruthy();
        });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/appointments' &&
          r.params.get('from') === '2026-01-01' &&
          r.params.get('to') === '2026-01-31' &&
          r.params.get('userId') === 'u1' &&
          r.params.get('customerId') === 'c1' &&
          r.params.get('pageNumber') === '2' &&
          r.params.get('pageSize') === '20'
      );
      expect(req.request.method).toBe('GET');
      req.flush({ items: [], totalCount: 0 });
    });
  });

  describe('getById', () => {
    it('should GET appointment by id', () => {
      service.getById('abc').subscribe((result) => {
        expect(result.id).toBe('abc');
      });

      const req = httpMock.expectOne('/api/appointments/abc');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 'abc' });
    });
  });

  describe('create', () => {
    it('should POST a new appointment', () => {
      const body = {
        customerId: 'c1',
        serviceId: 's1',
        userId: 'u1',
        vacancyId: 'v1',
        notes: 'test',
      } as any;

      service.create(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/appointments');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ id: '1' });
    });
  });

  describe('update', () => {
    it('should PUT an appointment', () => {
      const body = { notes: 'updated' } as any;

      service.update('a1', body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/appointments/a1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush({ id: 'a1' });
    });
  });

  describe('confirm', () => {
    it('should PATCH to confirm an appointment', () => {
      service.confirm('a1').subscribe();

      const req = httpMock.expectOne('/api/appointments/a1/confirm');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });

  describe('cancel', () => {
    it('should PATCH to cancel an appointment', () => {
      service.cancel('a1').subscribe();

      const req = httpMock.expectOne('/api/appointments/a1/cancel');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });

  describe('markDone', () => {
    it('should PATCH to mark an appointment as done', () => {
      service.markDone('a1').subscribe();

      const req = httpMock.expectOne('/api/appointments/a1/done');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });
});
