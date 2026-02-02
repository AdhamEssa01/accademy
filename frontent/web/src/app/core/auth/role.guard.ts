import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const requiredRoles = (route.data?.['roles'] as string[]) ?? [];

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (requiredRoles.length === 0) {
    return true;
  }

  const userRoles = auth.getUser()?.roles ?? [];
  const isAllowed = requiredRoles.some((role) => userRoles.includes(role));

  return isAllowed ? true : router.createUrlTree(['/forbidden']);
};
