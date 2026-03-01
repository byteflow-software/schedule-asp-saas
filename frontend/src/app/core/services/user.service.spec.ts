import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  provideHttpClientTesting,
  HttpTestingController,
} from '@angular/common/http/testing';
import { UserService } from './user.service';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should GET all users', () => {
      service.getAll().subscribe((result) => {
        expect(result.length).toBe(2);
      });

      const req = httpMock.expectOne('/api/users');
      expect(req.request.method).toBe('GET');
      req.flush([{ id: '1' }, { id: '2' }]);
    });
  });

  describe('create', () => {
    it('should POST a new user', () => {
      const body = {
        fullName: 'Jane Doe',
        email: 'jane@test.com',
        password: 'Pass123!',
        role: 'Professional',
      } as any;

      service.create(body).subscribe((result) => {
        expect(result).toBeTruthy();
      });

      const req = httpMock.expectOne('/api/users');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ id: '1', ...body });
    });
  });

  describe('deactivate', () => {
    it('should PATCH to deactivate a user', () => {
      service.deactivate('u1').subscribe();

      const req = httpMock.expectOne('/api/users/u1/deactivate');
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual({});
      req.flush(null);
    });
  });
});
