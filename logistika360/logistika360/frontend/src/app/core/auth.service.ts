import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
export const API='http://localhost:5000/api'; // set to your API URL
@Injectable({providedIn:'root'})
export class AuthService{
  private http=inject(HttpClient);
  role=signal<string|null>(sessionStorage.getItem('role'));
  get token(){return sessionStorage.getItem('token');}
  login(username:string,password:string){return this.http.post<{token:string;role:string}>(`${API}/auth/login`,{username,password}).pipe(tap(r=>{sessionStorage.setItem('token',r.token);sessionStorage.setItem('role',r.role);this.role.set(r.role);}));}
  logout(){sessionStorage.clear();this.role.set(null);}
  has(...r:string[]){return !!this.role()&&r.includes(this.role()!);}
}
