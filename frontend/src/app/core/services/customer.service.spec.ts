import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { CustomerService } from './customer.service';

describe('CustomerService', () => {
  let service: CustomerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CustomerService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(CustomerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET customers with default pagination', () => {
      service.getAll().subscribe((result) => {
        expect(result.items.length).toBe(1);
      });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/customers' &&
          r.params.get('pageNumber') === '1' &&
          r.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush({ items: [{ id: '1' }], totalCount: 1 });
    });

    it('should GET customers with search and custom pagination', () => {
      service.getAll('John', 2, 25).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne(
        (r) =>
          r.url === '/api/customers' &&
          r.params.get('search') === 'John' &&
          r.params.get('pageNumber') === '2' &&
          r.params.get('pageSize') === '25'
      );
      expect(req.request.method).toBe('GET');
      req.flush({ items: [], totalCount: 0 });
    });
  });

  describe('getById', () => {
    it('should GET customer by id', () => {
      service.getById('c1').subscribe((result) => {
        expect(result.id).toBe('c1');
      });

      const req = httpMock.expectOne('/api/customers/c1');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 'c1' });
    });
  });

  describe('create', () => {
    it('should POST a new customer', () => {
      const body = { name: 'John', email: 'john@test.com', phone: '123' } as any;

      service.create(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/customers');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ id: '1', ...body });
    });
  });

  describe('update', () => {
    it('should PUT a customer using request.id in URL', () => {
      const body = {
        id: 'c1',
        name: 'John Updated',
        email: 'john@test.com',
        phone: '123',
      } as any;

      service.update(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/customers/c1');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush({ id: 'c1' });
    });
  });

  describe('delete', () => {
    it('should DELETE a customer by id', () => {
      service.delete('c1').subscribe();

      const req = httpMock.expectOne('/api/customers/c1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
