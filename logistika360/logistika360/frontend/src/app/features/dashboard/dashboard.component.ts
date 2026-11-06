import { Component, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API } from '../../core/auth.service';
@Component({standalone:true,template:`
<h1>Dashboard</h1>
@if(d(); as k){<div class="kpis">
 <div class="kpi"><b>{{k.ordersToday}}</b>Orders today</div><div class="kpi"><b>{{k.activeOrders}}</b>Active orders</div>
 <div class="kpi"><b>{{k.sales7d}}</b>Sales, 7 days</div><div class="kpi"><b [class.low]="k.lowStock>0">{{k.lowStock}}</b>Low-stock items</div>
 <div class="kpi"><b>{{k.onTimeRate}}%</b>On-time delivery</div></div>}@else{<p>Loading…</p>}`})
export class DashboardComponent{d=signal<any>(null);constructor(h:HttpClient){h.get(`${API}/dashboard`).subscribe(x=>this.d.set(x));}}
