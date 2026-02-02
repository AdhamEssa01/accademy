import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const snackBar = inject(MatSnackBar);
  const router = inject(Router);
  const auth = inject(AuthService);

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = getMessage(error);
      if (message) {
        snackBar.open(message, 'Dismiss', { duration: 4000 });
      }
      if (error.status === 401) {
        auth.clearSession();
        void router.navigateByUrl('/login');
      }
      if (error.status === 403) {
        void router.navigateByUrl('/forbidden');
      }
      if (error.status === 404) {
        void router.navigateByUrl('/not-found');
      }
      return throwError(() => error);
    })
  );
};

function getMessage(error: HttpErrorResponse): string | null {
  const payload = error.error as ProblemDetails | string | undefined;
  if (!payload) {
    return 'Something went wrong.';
  }

  if (typeof payload === 'string') {
    return payload;
  }

  if (payload.errors) {
    const firstKey = Object.keys(payload.errors)[0];
    const firstError = firstKey ? payload.errors[firstKey]?.[0] : undefined;
    if (firstError) {
      return firstError;
    }
  }

  return payload.title || payload.detail || 'Something went wrong.';
}
