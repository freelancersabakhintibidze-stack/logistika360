using Microsoft.EntityFrameworkCore; using Logistika360.Api.Data; using Logistika360.Api.Models;
namespace Logistika360.Api.Services;
public class AppException(int code,string msg):Exception(msg){public int Code=code;}
public class OrderService(AppDbContext db){
 static readonly Dictionary<string,Dictionary<string,string[]>> Flow=new(){
  ["Draft"]=new(){["Submitted"]=[Roles.Operator,Roles.Admin],["Cancelled"]=[Roles.Operator,Roles.Admin]},
  ["Submitted"]=new(){["Approved"]=[Roles.Manager,Roles.Admin],["Cancelled"]=[Roles.Manager,Roles.Admin]},
  ["Approved"]=new(){["Picking"]=[Roles.Manager,Roles.Admin],["Cancelled"]=[Roles.Manager,Roles.Admin]},
  ["Picking"]=new(){["Packed"]=[Roles.Manager,Roles.Admin]},
  ["Packed"]=new(){["Dispatched"]=[Roles.Manager,Roles.Admin,Roles.Courier]},
  ["Dispatched"]=new(){["Delivered"]=[Roles.Courier,Roles.Admin]}};
 public async Task<Order> CreateDraft(OrderCreateDto d,string user){
  if(d.Items.Count==0||d.Items.Any(i=>i.Quantity<=0)) throw new AppException(400,"Items required; quantity must be positive.");
  var o=new Order{CustomerId=d.CustomerId,CreatedBy=user};
  foreach(var i in d.Items){ var p=await db.Products.FindAsync(i.ProductId)??throw new AppException(404,$"Product {i.ProductId} not found");
   if(!p.IsActive) throw new AppException(400,$"Product {p.Sku} is inactive.");
   o.Items.Add(new OrderItem{ProductId=p.Id,WarehouseId=i.WarehouseId,Quantity=i.Quantity,UnitPrice=p.Price});}
  o.TotalAmount=o.Items.Sum(i=>i.Quantity*i.UnitPrice); db.Orders.Add(o);
  db.AuditEvents.Add(new AuditEvent{UserName=user,Entity="Order",Action="Create"}); await db.SaveChangesAsync(); return o;}
 public async Task<Order> Transition(int id,string next,string role,string user){
  var o=await db.Orders.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==id)??throw new AppException(404,"Order not found");
  if(!Flow.TryGetValue(o.Status,out var m)||!m.TryGetValue(next,out var roles)) throw new AppException(400,$"Invalid transition {o.Status} -> {next}.");
  if(!roles.Contains(role)) throw new AppException(403,"Role not allowed for this transition.");
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
  try{
   if(next=="Submitted") foreach(var i in o.Items){
     var s=await db.Stock.FirstOrDefaultAsync(x=>x.ProductId==i.ProductId&&x.WarehouseId==i.WarehouseId)??throw new AppException(400,"No stock record.");
     if(s.Quantity-s.ReservedQuantity<i.Quantity) throw new AppException(409,$"Insufficient stock for product {i.ProductId}: available {s.Quantity-s.ReservedQuantity}.");
     s.ReservedQuantity+=i.Quantity;}
   if(next=="Cancelled"&&o.Status!="Draft") await Release(o,false);
   if(next=="Delivered") await Release(o,true);
   o.Status=next; db.AuditEvents.Add(new AuditEvent{UserName=user,Entity="Order",Action="Status",Details=$"{id}->{next}"});
   await db.SaveChangesAsync(); await tx.CommitAsync(); return o;
  }catch(DbUpdateConcurrencyException){ throw new AppException(409,"Stock changed concurrently; retry."); }}
 async Task Release(Order o,bool consume){ foreach(var i in o.Items){ var s=await db.Stock.FirstAsync(x=>x.ProductId==i.ProductId&&x.WarehouseId==i.WarehouseId);
   s.ReservedQuantity-=i.Quantity; if(consume) s.Quantity-=i.Quantity; }}
}
