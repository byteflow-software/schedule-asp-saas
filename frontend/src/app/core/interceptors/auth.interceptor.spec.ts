import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpHandlerFn, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let authServiceMock: {
    getAccessToken: jest.Mock;
    tenantId: jest.Mock;
  };

  const mockNext: HttpHandlerFn = (req) =>
    of(new HttpResponse({ status: 200, body: {} }));

  beforeEach(() => {
    authServiceMock = {
      getAccessToken: jest.fn(),
      tenantId: jest.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
      ],
    });
  });

  it('should add Authorization and X-Tenant-Id headers when token and tenantId exist', (done) => {
    authServiceMock.getAccessToken.mockReturnValue('my-token');
    authServiceMock.tenantId.mockReturnValue('tenant-123');

    const req = new HttpRequest('GET', '/api/test');

    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (interceptedReq) => {
        expect(interceptedReq.headers.get('Authorization')).toBe('Bearer my-token');
        expect(interceptedReq.headers.get('X-Tenant-Id')).toBe('tenant-123');
        return mockNext(interceptedReq);
      }).subscribe(() => done());
    });
  });

  it('should add only Authorization header when tenantId is empty', (done) => {
    authServiceMock.getAccessToken.mockReturnValue('my-token');
    authServiceMock.tenantId.mockReturnValue('');

    const req = new HttpRequest('GET', '/api/test');

    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (interceptedReq) => {
        expect(interceptedReq.headers.get('Authorization')).toBe('Bearer my-token');
        expect(interceptedReq.headers.has('X-Tenant-Id')).toBe(false);
        return mockNext(interceptedReq);
      }).subscribe(() => done());
    });
  });

  it('should pass through without headers when no token exists', (done) => {
    authServiceMock.getAccessToken.mockReturnValue(null);
    authServiceMock.tenantId.mockReturnValue('');

    const req = new HttpRequest('GET', '/api/test');

    TestBed.runInInjectionContext(() => {
      authInterceptor(req, (interceptedReq) => {
        expect(interceptedReq.headers.has('Authorization')).toBe(false);
        expect(interceptedReq.headers.has('X-Tenant-Id')).toBe(false);
        return mockNext(interceptedReq);
      }).subscribe(() => done());
    });
  });
});
