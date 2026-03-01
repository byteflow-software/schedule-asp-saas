import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { adminGuard } from './role.guard';
import { AuthService } from '../services/auth.service';

describe('adminGuard', () => {
  let authServiceMock: { isAdmin: jest.Mock };
  let routerMock: { navigate: jest.Mock };

  const mockRoute = {} as ActivatedRouteSnapshot;
  const mockState = {} as RouterStateSnapshot;

  beforeEach(() => {
    authServiceMock = {
      isAdmin: jest.fn(),
    };

    routerMock = {
      navigate: jest.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    });
  });

  it('should return true when user is admin', () => {
    authServiceMock.isAdmin.mockReturnValue(true);

    const result = TestBed.runInInjectionContext(() => adminGuard(mockRoute, mockState));

    expect(result).toBe(true);
    expect(routerMock.navigate).not.toHaveBeenCalled();
  });

  it('should navigate to root and return false when user is not admin', () => {
    authServiceMock.isAdmin.mockReturnValue(false);

    const result = TestBed.runInInjectionContext(() => adminGuard(mockRoute, mockState));

    expect(result).toBe(false);
    expect(routerMock.navigate).toHaveBeenCalledWith(['/']);
  });
});
