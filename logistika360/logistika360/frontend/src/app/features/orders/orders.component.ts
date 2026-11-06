import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API, AuthService } from '../../core/auth.service';
import { Order, Page } from '../../models/models';
const NEXT:Record<string,string[]>={Draft:['Submitted','Cancelled'],Submitted:['Approved','Cancelled'],Approved:['Picking','Cancelled'],Picking:['Packed'],Packed:['Dispatched'],Dispatched:['Delivered']};
@Component({standalone:true,template:`
<h1>Orders</h1>@if(err()){<p class="err" role="alert">{{err()}}</p>}
<table><thead><tr><th>#</th><th>Customer</th><th>Status</th><th>Total</th><th>Actions</th></tr></thead><tbody>
@for(o of items();track o.id){<tr><td>{{o.id}}</td><td>{{o.customerId}}</td><td>{{o.status}}</td><td>{{o.totalAmount}}</td><td>
 @if(!auth.has('Viewer')){@for(n of next(o.status);track n){<button class="btn" [disabled]="busy()" (click)="move(o,n)">{{n}}</button> }}</td></tr>}
@empty{<tr><td colspan="5">No orders yet.</td></tr>}</tbody></table>`})
export class OrdersComponent{h=inject(HttpClient);auth=inject(AuthService);items=signal<Order[]>([]);err=signal('');busy=signal(false);
 constructor(){this.load();} next=(s:string)=>NEXT[s]??[];
 load(){this.h.get<Page<Order>>(`${API}/orders`).subscribe(r=>this.items.set(r.items));}
 move(o:Order,s:string){if(this.busy())return;this.busy.set(true);
  this.h.post(`${API}/orders/${o.id}/status`,{status:s}).subscribe({next:()=>{this.busy.set(false);this.err.set('');this.load();},
   error:(e:any)=>{this.busy.set(false);this.err.set(`${e.error?.error??'Update failed.'} (ref ${e.error?.correlationId??'n/a'})`);}});}}
