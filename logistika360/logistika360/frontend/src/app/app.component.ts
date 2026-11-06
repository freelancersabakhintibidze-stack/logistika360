import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';
@Component({selector:'app-root',standalone:true,imports:[RouterOutlet,RouterLink,RouterLinkActive],template:`
@if(auth.role()){<nav aria-label="Main"><strong>Logistika 360</strong>
 <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
 <a routerLink="/products" routerLinkActive="active">Products</a>
 @if(!auth.has('Courier')){<a routerLink="/orders" routerLinkActive="active">Orders</a>}
 <a routerLink="/deliveries" routerLinkActive="active">Deliveries</a>
 <span style="flex:1"></span><span>{{auth.role()}}</span><button (click)="out()">Sign out</button></nav>}
<main><router-outlet/></main>`})
export class AppComponent{auth=inject(AuthService);r=inject(Router);out(){this.auth.logout();this.r.navigate(['/login']);}}
