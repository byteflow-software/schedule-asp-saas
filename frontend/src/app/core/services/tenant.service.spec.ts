import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { TenantService } from './tenant.service';

describe('TenantService', () => {
  let service: TenantService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TenantService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(TenantService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getMyTenant', () => {
    it('should GET the current tenant', () => {
      service.getMyTenant().subscribe((result) => {
        expect(result.id).toBe('t1');
      });

      const req = httpMock.expectOne('/api/tenants/me');
      expect(req.request.method).toBe('GET');
      req.flush({ id: 't1', name: 'My Tenant' });
    });
  });

  describe('updateTenant', () => {
    it('should PUT tenant data', () => {
      const body = { name: 'Updated Tenant' } as any;

      service.updateTenant(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/tenants/me');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush({ id: 't1', name: 'Updated Tenant' });
    });
  });

  describe('validateAsaasKey', () => {
    it('should POST to validate an Asaas API key', () => {
      service.validateAsaasKey('key_abc123').subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/tenants/me/validate-asaas');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ apiKey: 'key_abc123' });
      req.flush({ id: 't1' });
    });
  });
});
