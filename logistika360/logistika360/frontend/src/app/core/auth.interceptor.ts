import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
export const authInterceptor:HttpInterceptorFn=(req,next)=>{
  const a=inject(AuthService),r=inject(Router);
  const t=a.token; if(t) req=req.clone({setHeaders:{Authorization:`Bearer ${t}`}});
  return next(req).pipe(catchError(e=>{if(e.status===401){a.logout();r.navigate(['/login']);}return throwError(()=>e);}));
};
