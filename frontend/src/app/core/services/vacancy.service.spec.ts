import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { VacancyService } from './vacancy.service';

describe('VacancyService', () => {
  let service: VacancyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        VacancyService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(VacancyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET vacancies without filters', () => {
      service.getAll().subscribe((result) => {
        expect(result.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/vacancies');
      expect(req.request.method).toBe('GET');
      req.flush([{ id: '1' }]);
    });

    it('should GET vacancies with filters', () => {
      service
        .getAll({
          userId: 'u1',
          serviceId: 's1',
          from: '2026-01-01',
          to: '2026-01-31',
          available: true,
        })
        .subscribe((result) => {
          expect(result).toBeTruthy();
        });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/vacancies' &&
          r.params.get('userId') === 'u1' &&
          r.params.get('serviceId') === 's1' &&
          r.params.get('from') === '2026-01-01' &&
          r.params.get('to') === '2026-01-31' &&
          r.params.get('available') === 'true'
      );
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('create', () => {
    it('should POST a new vacancy', () => {
      const body = {
        userId: 'u1',
        serviceId: 's1',
        startTime: '2026-01-15T09:00:00',
      } as any;

      service.create(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/vacancies');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ id: '1', ...body });
    });
  });

  describe('bulkCreate', () => {
    it('should POST bulk vacancies', () => {
      const body = {
        userId: 'u1',
        serviceId: 's1',
        slots: [],
      } as any;

      service.bulkCreate(body).subscribe((result) => {
        expect(result.length).toBe(2);
      });

      const req = httpMock.expectOne('/api/vacancies/bulk');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush([{ id: '1' }, { id: '2' }]);
    });
  });

  describe('delete', () => {
    it('should DELETE a vacancy by id', () => {
      service.delete('v1').subscribe();

      const req = httpMock.expectOne('/api/vacancies/v1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
