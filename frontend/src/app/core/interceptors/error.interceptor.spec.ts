import { TestBed } from '@angular/core/testing';
import { HttpRequest, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { errorInterceptor } from './error.interceptor';
import { AuthService } from '../services/auth.service';

describe('errorInterceptor', () => {
  let snackBarMock: { open: jest.Mock };
  let authServiceMock: { logout: jest.Mock };

  beforeEach(() => {
    snackBarMock = { open: jest.fn() };
    authServiceMock = { logout: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: MatSnackBar, useValue: snackBarMock },
        { provide: AuthService, useValue: authServiceMock },
      ],
    });
  });

  it('should pass through non-error responses', (done) => {
    const req = new HttpRequest('GET', '/api/test');
    const mockNext = () => of(new HttpResponse({ status: 200, body: { ok: true } }));

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe((response) => {
        expect(response).toBeTruthy();
        expect(authServiceMock.logout).not.toHaveBeenCalled();
        expect(snackBarMock.open).not.toHaveBeenCalled();
        done();
      });
    });
  });

  it('should call logout on 401 error for non-auth URLs', (done) => {
    const req = new HttpRequest('GET', '/api/data');
    const error = new HttpErrorResponse({ status: 401, url: '/api/data' });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(authServiceMock.logout).toHaveBeenCalled();
          expect(snackBarMock.open).not.toHaveBeenCalled();
          done();
        },
      });
    });
  });

  it('should not call logout on 401 error for auth URLs', (done) => {
    const req = new HttpRequest('POST', '/api/auth/login');
    const error = new HttpErrorResponse({ status: 401, url: '/api/auth/login' });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(authServiceMock.logout).not.toHaveBeenCalled();
          done();
        },
      });
    });
  });

  it('should show snackbar with default message on 400 error', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({ status: 400, url: '/api/test', error: {} });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Requisição inválida.', 'OK', expect.objectContaining({ duration: 6000 }));
          done();
        },
      });
    });
  });

  it('should show validation errors on 400 with errors object', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { errors: { Name: ['Name is required'], Email: ['Email is invalid'] } },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Name is required Email is invalid', 'OK', expect.objectContaining({ duration: 6000 }));
          done();
        },
      });
    });
  });

  it('should show body.message on 400 when not generic validation error', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { message: 'Custom error message' },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Custom error message', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show default on 400 when message is generic validation message', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { message: 'One or more validation errors occurred.' },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Requisição inválida.', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show body.detail on 400', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { detail: 'Detailed error info' },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Detailed error info', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show body.title on 400', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { title: 'Error Title' },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Error Title', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show snackbar on 400 with empty errors array', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({
      status: 400, url: '/api/test',
      error: { errors: { field: [] } },
    });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Requisição inválida.', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show snackbar on 403 error for non-users URLs', (done) => {
    const req = new HttpRequest('GET', '/api/settings');
    const error = new HttpErrorResponse({ status: 403, url: '/api/settings' });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Você não tem permissão para esta ação.', 'OK', expect.objectContaining({ duration: 4000 }));
          done();
        },
      });
    });
  });

  it('should not show snackbar on 403 for /api/users URLs', (done) => {
    const req = new HttpRequest('GET', '/api/users');
    const error = new HttpErrorResponse({ status: 403, url: '/api/users' });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).not.toHaveBeenCalled();
          done();
        },
      });
    });
  });

  it('should show snackbar on 404 error', (done) => {
    const req = new HttpRequest('GET', '/api/test/999');
    const error = new HttpErrorResponse({ status: 404, url: '/api/test/999', error: { message: 'Not found' } });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Not found', 'OK', expect.objectContaining({ duration: 4000 }));
          done();
        },
      });
    });
  });

  it('should show default 404 message when no error message', (done) => {
    const req = new HttpRequest('GET', '/api/test/999');
    const error = new HttpErrorResponse({ status: 404, url: '/api/test/999', error: {} });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Recurso não encontrado.', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show snackbar on 409 conflict error', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({ status: 409, url: '/api/test', error: { message: 'Duplicate' } });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Duplicate', 'OK', expect.objectContaining({ duration: 5000 }));
          done();
        },
      });
    });
  });

  it('should show default 409 message when no error message', (done) => {
    const req = new HttpRequest('POST', '/api/test');
    const error = new HttpErrorResponse({ status: 409, url: '/api/test', error: {} });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Conflito de dados.', 'OK', expect.any(Object));
          done();
        },
      });
    });
  });

  it('should show connection error on status 0', (done) => {
    const req = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 0, url: '/api/test' });
    const mockNext = () => throwError(() => error);

    TestBed.runInInjectionContext(() => {
      errorInterceptor(req, mockNext).subscribe({
        error: () => {
          expect(snackBarMock.open).toHaveBeenCalledWith('Servidor indisponível. Verifique sua conexão.', 'OK', expect.objectContaining({ duration: 5000 }));
          done();
        },
      });
    });
  });
});
