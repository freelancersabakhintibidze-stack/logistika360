import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
export const routes:Routes=[
 {path:'login',loadComponent:()=>import('./features/login/login.component').then(m=>m.LoginComponent)},
 {path:'dashboard',canActivate:[authGuard()],loadComponent:()=>import('./features/dashboard/dashboard.component').then(m=>m.DashboardComponent)},
 {path:'products',canActivate:[authGuard()],loadComponent:()=>import('./features/products/products.component').then(m=>m.ProductsComponent)},
 {path:'orders',canActivate:[authGuard('Admin','Operator','WarehouseManager','Viewer')],loadComponent:()=>import('./features/orders/orders.component').then(m=>m.OrdersComponent)},
 {path:'deliveries',canActivate:[authGuard()],loadComponent:()=>import('./features/deliveries/deliveries.component').then(m=>m.DeliveriesComponent)},
 {path:'**',redirectTo:'dashboard'}];
