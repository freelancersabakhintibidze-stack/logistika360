import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { API } from '../../core/auth.service';
import { Page, Product } from '../../models/models';
@Component({standalone:true,imports:[FormsModule],template:`
<h1>Product catalog</h1>
<input name="q" placeholder="Search SKU or name" aria-label="Search" [(ngModel)]="q" (ngModelChange)="reset()">
<select name="a" aria-label="Status" [(ngModel)]="active" (ngModelChange)="reset()"><option value="">All</option><option value="true">Active</option><option value="false">Inactive</option></select>
@if(err()){<p class="err" role="alert">{{err()}}</p>}
<table><thead><tr><th>SKU</th><th>Name</th><th>Category</th><th>Price</th><th>Status</th></tr></thead><tbody>
@for(p of items();track p.id){<tr><td>{{p.sku}}</td><td>{{p.name}}</td><td>{{p.category}}</td><td>{{p.price}}</td><td>{{p.isActive?'Active':'Inactive'}}</td></tr>}
@empty{<tr><td colspan="5">No products match these filters.</td></tr>}</tbody></table>
<p><button class="btn" [disabled]="page===1" (click)="go(page-1)">Previous</button> Page {{page}} of {{pages()}}
<button class="btn" [disabled]="page>=pages()" (click)="go(page+1)">Next</button></p>`})
export class ProductsComponent{h=inject(HttpClient);q='';active='';page=1;size=20;items=signal<Product[]>([]);total=signal(0);err=signal('');
 pages=()=>Math.max(1,Math.ceil(this.total()/this.size));constructor(){this.load();}
 reset(){this.page=1;this.load();} go(p:number){this.page=p;this.load();}
 load(){let u=`${API}/products?page=${this.page}&pageSize=${this.size}&q=${encodeURIComponent(this.q)}`;if(this.active)u+=`&active=${this.active}`;
  this.h.get<Page<Product>>(u).subscribe({next:r=>{this.items.set(r.items);this.total.set(r.total);this.err.set('');},error:e=>this.err.set(e.error?.error??'Could not load products.')});}}
