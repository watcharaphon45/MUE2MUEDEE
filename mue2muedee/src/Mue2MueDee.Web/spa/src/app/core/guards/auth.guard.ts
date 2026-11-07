import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};

export const superAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.isSuperAdmin() || inject(Router).createUrlTree(['/dashboard']);
};

export const storeContextGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) return router.createUrlTree(['/login']);
  if (state.url.split('?')[0] === '/super-admin/stores') {
    return auth.isSuperAdmin() || router.createUrlTree(['/dashboard']);
  }
  if (auth.currentStore()) return true;
  return router.createUrlTree([auth.isSuperAdmin() ? '/super-admin/stores' : '/login']);
};
