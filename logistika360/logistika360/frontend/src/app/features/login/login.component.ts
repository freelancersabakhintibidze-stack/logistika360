import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
@Component({standalone:true,imports:[FormsModule],template:`
<h1>Sign in</h1>
<label>Username <input name="u" [(ngModel)]="u" autocomplete="username"></label><br><br>
<label>Password <input name="p" type="password" [(ngModel)]="p" autocomplete="current-password" (keyup.enter)="go()"></label><br><br>
<button class="btn" [disabled]="busy()" (click)="go()">Sign in</button>
@if(err()){<p class="err" role="alert">{{err()}}</p>}`})
export class LoginComponent{a=inject(AuthService);r=inject(Router);u='';p='';busy=signal(false);err=signal('');
 go(){if(this.busy())return;this.busy.set(true);this.a.login(this.u,this.p).subscribe({next:()=>this.r.navigate(['/dashboard']),error:()=>{this.err.set('Username or password is incorrect.');this.busy.set(false);}});}}
