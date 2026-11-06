import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API, AuthService } from '../../core/auth.service';
import { Delivery } from '../../models/models';
@Component({standalone:true,template:`
<h1>Deliveries</h1>@if(err()){<p class="err" role="alert">{{err()}}</p>}
<table><thead><tr><th>Order</th><th>Status</th><th>Address</th><th>Phone</th><th>Update</th></tr></thead><tbody>
@for(d of items();track d.id){<tr><td>{{d.orderId}}</td><td>{{d.status}}</td><td>{{d.address??'Hidden'}}</td><td>{{d.phone??'Hidden'}}</td><td>
 @if(!auth.has('Viewer')){<button class="btn" (click)="set(d,'InTransit')">In transit</button> <button class="btn" (click)="set(d,'Delivered')">Delivered</button> <button class="btn" (click)="fail(d)">Failed</button>}</td></tr>}
@empty{<tr><td colspan="5">No deliveries assigned.</td></tr>}</tbody></table>`})
export class DeliveriesComponent{h=inject(HttpClient);auth=inject(AuthService);items=signal<Delivery[]>([]);err=signal('');
 constructor(){this.load();} load(){this.h.get<Delivery[]>(`${API}/deliveries`).subscribe(r=>this.items.set(r));}
 set(d:Delivery,status:string,failReason?:string){this.h.put(`${API}/deliveries/${d.id}`,{status,failReason}).subscribe({next:()=>this.load(),error:(e:any)=>this.err.set(e.error?.error??'Update failed.')});}
 fail(d:Delivery){const r=prompt('Reason (e.g. customer unreachable, incorrect address)');if(r?.trim())this.set(d,'Failed',r.trim());}}
