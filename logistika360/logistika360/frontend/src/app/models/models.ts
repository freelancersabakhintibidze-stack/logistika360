export interface Product{id:number;sku:string;name:string;category?:string;price:number;isActive:boolean}
export interface Page<T>{total:number;items:T[]}
export interface Order{id:number;customerId:number;status:string;totalAmount:number;createdAt:string}
export interface Delivery{id:number;orderId:number;courierId?:number;status:string;address?:string;phone?:string;failReason?:string}
