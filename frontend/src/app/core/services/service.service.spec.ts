import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { ServiceService } from './service.service';

describe('ServiceService', () => {
  let service: ServiceService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ServiceService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ServiceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET all services', () => {
      service.getAll().subscribe((result) => {
        expect(result.length).toBe(2);
      });

      const req = httpMock.expectOne('/api/services');
      expect(req.request.method).toBe('GET');
      req.flush([{ id: '1' }, { id: '2' }]);
    });
  });

  describe('getById', () => {
    it('should GET service by id', () => {
      service.getById('s1').subscribe((result) => {
        expect(result.id).toBe('s1');
      });

      const req = httpMock.expectOne('/api/services/s1');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 's1' });
    });
  });

  describe('create', () => {
    it('should POST a new service', () => {
      const body = {
        name: 'Haircut',
        durationMinutes: 30,
        priceInCents: 5000,
      } as any;

      service.create(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/services');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ id: '1', ...body });
    });
  });

  describe('update', () => {
    it('should PUT a service using request.id in URL', () => {
      const body = {
        id: 's1',
        name: 'Haircut Updated',
        durationMinutes: 45,
        priceInCents: 6000,
      } as any;

      service.update(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/services/s1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush({ id: 's1' });
    });
  });

  describe('delete', () => {
    it('should DELETE a service by id', () => {
      service.delete('s1').subscribe();

      const req = httpMock.expectOne('/api/services/s1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
