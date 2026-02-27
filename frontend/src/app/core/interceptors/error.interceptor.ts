import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { tap } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const authService = inject(AuthService);

  return next(req).pipe(
    tap({
      error: (error) => {
        switch (error.status) {
          case 401:
            if (!req.url.includes('/auth/')) {
              authService.logout();
            }
            break;
          case 403:
            if (!req.url.includes('/api/users')) {
              snackBar.open('Você não tem permissão para esta ação.', 'OK', {
                duration: 4000,
                panelClass: ['snackbar-error'],
              });
            }
            break;
          case 400: {
            let message = 'Requisição inválida.';
            const body = error.error;
            if (body?.errors && typeof body.errors === 'object') {
              const allErrors = Object.values(body.errors).flat() as string[];
              if (allErrors.length > 0) {
                message = allErrors.join(' ');
              }
            } else if (body?.message && body.message !== 'One or more validation errors occurred.') {
              message = body.message;
            } else if (body?.detail) {
              message = body.detail;
            } else if (body?.title) {
              message = body.title;
            }
            snackBar.open(message, 'OK', {
              duration: 6000,
              panelClass: ['snackbar-error'],
            });
            break;
          }
          case 404:
            snackBar.open(error.error?.message || 'Recurso não encontrado.', 'OK', {
              duration: 4000,
              panelClass: ['snackbar-error'],
            });
            break;
          case 409:
            snackBar.open(error.error?.message || 'Conflito de dados.', 'OK', {
              duration: 5000,
              panelClass: ['snackbar-error'],
            });
            break;
          case 0:
            snackBar.open('Servidor indisponível. Verifique sua conexão.', 'OK', {
              duration: 5000,
              panelClass: ['snackbar-error'],
            });
            break;
        }
      },
    })
  );
};
