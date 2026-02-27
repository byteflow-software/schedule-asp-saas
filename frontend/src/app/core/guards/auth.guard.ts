import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  // If token is expired, try refreshing before blocking
  if (authService.isTokenExpired()) {
    return authService.refreshToken().pipe(
      map((tokens) => {
        if (tokens) {
          return true;
        }
        authService.logout();
        return false;
      })
    );
  }

  return true;
};
