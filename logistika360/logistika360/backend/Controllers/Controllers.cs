using System.Security.Claims; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
using Logistika360.Api.Data; using Logistika360.Api.Models; using Logistika360.Api.Services;
namespace Logistika360.Api.Controllers;
public class TokenService(IConfiguration c){ public string Make(User u){
 var key=new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(c["Jwt:Key"]!));
 var t=new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(c["Jwt:Issuer"],null,[new Claim(ClaimTypes.Name,u.Username),new Claim(ClaimTypes.Role,u.Role),new Claim("uid",u.Id.ToString())],expires:DateTime.UtcNow.AddHours(8),signingCredentials:new(key,"HS256"));
 return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(t);}}
[ApiController,Route("api/auth")] public class AuthController(AppDbContext db,TokenService ts):ControllerBase{
 [HttpPost("login")] public async Task<IActionResult> Login(LoginDto d){ var u=await db.Users.FirstOrDefaultAsync(x=>x.Username==d.Username);
  if(u==null||!BCrypt.Net.BCrypt.Verify(d.Password,u.PasswordHash)) return Unauthorized(new{error="Invalid credentials"});
  return Ok(new{token=ts.Make(u),role=u.Role,username=u.Username});}}
[ApiController,Authorize,Route("api/products")] public class ProductsController(AppDbContext db):ControllerBase{
 [HttpGet] public async Task<IActionResult> List(string? q,string? category,bool? active,int page=1,int pageSize=20,string sort="name"){
  var x=db.Products.AsNoTracking().AsQueryable();
  if(!string.IsNullOrWhiteSpace(q)) x=x.Where(p=>p.Sku.Contains(q)||p.Name.Contains(q));
  if(!string.IsNullOrWhiteSpace(category)) x=x.Where(p=>p.Category==category); if(active.HasValue) x=x.Where(p=>p.IsActive==active);
  x=sort switch{"sku"=>x.OrderBy(p=>p.Sku),"price"=>x.OrderBy(p=>p.Price),_=>x.OrderBy(p=>p.Name)};
  pageSize=Math.Clamp(pageSize,1,100); var total=await x.CountAsync();
  return Ok(new{total,items=await x.Skip((Math.Max(page,1)-1)*pageSize).Take(pageSize).ToListAsync()});}
 [HttpGet("{id}")] public async Task<IActionResult> Get(int id)=>await db.Products.FindAsync(id) is{} p?Ok(p):NotFound();
 [HttpPost,Authorize(Roles="Admin,WarehouseManager")] public async Task<IActionResult> Create(Product p){
  if(string.IsNullOrWhiteSpace(p.Sku)||p.Price<0||p.Name.Length>150) return BadRequest(new{error="Invalid product data."});
  if(await db.Products.AnyAsync(x=>x.Sku==p.Sku)) return Conflict(new{error=$"SKU '{p.Sku}' already exists."});
  p.Id=0; db.Products.Add(p); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get),new{id=p.Id},p);}
 [HttpPut("{id}"),Authorize(Roles="Admin,WarehouseManager")] public async Task<IActionResult> Update(int id,Product p){
  var e=await db.Products.FindAsync(id); if(e==null) return NotFound();
  if(p.Price<0||string.IsNullOrWhiteSpace(p.Sku)) return BadRequest(new{error="Invalid product data."});
  if(await db.Products.AnyAsync(x=>x.Sku==p.Sku&&x.Id!=id)) return Conflict(new{error="Duplicate SKU."});
  e.Sku=p.Sku;e.Name=p.Name;e.Category=p.Category;e.Price=p.Price;e.IsActive=p.IsActive; await db.SaveChangesAsync(); return Ok(e);}}
[ApiController,Authorize,Route("api/stock")] public class StockController(AppDbContext db):ControllerBase{
 [HttpGet] public async Task<IActionResult> List(int? warehouseId)=>Ok(await db.Stock.AsNoTracking().Where(s=>warehouseId==null||s.WarehouseId==warehouseId)
  .Select(s=>new{s.ProductId,s.WarehouseId,s.Quantity,s.ReservedQuantity,Available=s.Quantity-s.ReservedQuantity,Deficit=s.Quantity-s.ReservedQuantity<=0}).ToListAsync());}
[ApiController,Authorize,Route("api/orders")] public class OrdersController(AppDbContext db,OrderService svc):ControllerBase{
 string Role=>User.FindFirstValue(ClaimTypes.Role)!; string Name=>User.Identity!.Name!;
 [HttpGet] public async Task<IActionResult> List(string? status,int page=1,int pageSize=20){ var x=db.Orders.AsNoTracking().Where(o=>status==null||o.Status==status).OrderByDescending(o=>o.CreatedAt);
  return Ok(new{total=await x.CountAsync(),items=await x.Skip((Math.Max(page,1)-1)*pageSize).Take(Math.Clamp(pageSize,1,100)).ToListAsync()});}
 [HttpGet("{id}")] public async Task<IActionResult> Get(int id)=>await db.Orders.Include(o=>o.Items).FirstOrDefaultAsync(o=>o.Id==id) is{} o?Ok(o):NotFound();
 [HttpPost,Authorize(Roles="Admin,Operator")] public async Task<IActionResult> Create(OrderCreateDto d)=>Created("",await svc.CreateDraft(d,Name));
 [HttpPost("{id}/status")] public async Task<IActionResult> Status(int id,StatusDto d)=>Ok(await svc.Transition(id,d.Status,Role,Name));}
[ApiController,Authorize,Route("api/deliveries")] public class DeliveriesController(AppDbContext db):ControllerBase{
 [HttpGet] public async Task<IActionResult> List(){ var r=User.FindFirstValue(ClaimTypes.Role); var uid=int.Parse(User.FindFirstValue("uid")!);
  var x=db.Deliveries.AsNoTracking().AsQueryable(); if(r==Roles.Courier) x=x.Where(d=>d.CourierId==uid);
  var list=await x.ToListAsync(); if(r==Roles.Viewer) list.ForEach(d=>{d.Address=null;d.Phone=null;}); return Ok(list);}
 [HttpPut("{id}"),Authorize(Roles="Admin,WarehouseManager,Courier")] public async Task<IActionResult> Update(int id,DeliveryUpdateDto d){
  var e=await db.Deliveries.FindAsync(id); if(e==null) return NotFound(); var r=User.FindFirstValue(ClaimTypes.Role);
  if(r==Roles.Courier&&e.CourierId!=int.Parse(User.FindFirstValue("uid")!)) return Forbid();
  if(!new[]{"Pending","Assigned","InTransit","Delivered","Failed"}.Contains(d.Status)) return BadRequest(new{error="Invalid status."});
  if(d.Status=="Failed"&&string.IsNullOrWhiteSpace(d.FailReason)) return BadRequest(new{error="Failure reason is required."});
  if(r!=Roles.Courier){ e.CourierId=d.CourierId??e.CourierId; e.ScheduledAt=d.ScheduledAt??e.ScheduledAt; }
  e.Status=d.Status; e.FailReason=d.FailReason; if(d.Status is "Delivered" or "Failed") e.CompletedAt=DateTime.UtcNow;
  db.AuditEvents.Add(new AuditEvent{UserName=User.Identity!.Name,Entity="Delivery",Action="Update",Details=$"{id}->{d.Status}"}); await db.SaveChangesAsync(); return Ok(e);}}
[ApiController,Authorize,Route("api/dashboard")] public class DashboardController(AppDbContext db):ControllerBase{
 [HttpGet] public async Task<IActionResult> Get(){ var today=DateTime.UtcNow.Date; var wk=today.AddDays(-7);
  var done=await db.Deliveries.Where(d=>d.Status=="Delivered"&&d.CompletedAt!=null&&d.ScheduledAt!=null).Select(d=>d.CompletedAt<=d.ScheduledAt).ToListAsync();
  return Ok(new{ordersToday=await db.Orders.CountAsync(o=>o.CreatedAt>=today),
   activeOrders=await db.Orders.CountAsync(o=>o.Status!="Delivered"&&o.Status!="Cancelled"&&o.Status!="Draft"),
   sales7d=await db.Orders.Where(o=>o.CreatedAt>=wk&&o.Status!="Cancelled"&&o.Status!="Draft").SumAsync(o=>(decimal?)o.TotalAmount)??0,
   lowStock=await db.Stock.CountAsync(s=>s.Quantity-s.ReservedQuantity<=5),
   onTimeRate=done.Count==0?0:Math.Round(100.0*done.Count(x=>x)/done.Count,1)});}}
[ApiController,Authorize(Roles="Admin"),Route("api/audit")] public class AuditController(AppDbContext db):ControllerBase{
 [HttpGet] public async Task<IActionResult> List()=>Ok(await db.AuditEvents.OrderByDescending(a=>a.At).Take(200).ToListAsync());}
