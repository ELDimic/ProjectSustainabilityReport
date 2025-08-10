import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export function need(func: string): CanActivateFn {
  return () => {
    const auth = inject(AuthService); const router = inject(Router);
    if (!auth.token) { router.navigateByUrl('/login'); return false; }
    if (!auth.has(func)) { router.navigateByUrl('/'); return false; }
    return true;
  }
}
