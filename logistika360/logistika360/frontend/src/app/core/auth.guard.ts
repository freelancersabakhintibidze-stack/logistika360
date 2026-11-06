import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
export const authGuard=(...roles:string[]):CanActivateFn=>()=>{
  const a=inject(AuthService),r=inject(Router);
  if(!a.token) return r.parseUrl('/login');
  return roles.length===0||a.has(...roles)||r.parseUrl('/dashboard');
};
