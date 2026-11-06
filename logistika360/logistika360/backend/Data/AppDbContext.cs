using Microsoft.EntityFrameworkCore; using Logistika360.Api.Models;
namespace Logistika360.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> o):DbContext(o){
 public DbSet<Product> Products=>Set<Product>(); public DbSet<Warehouse> Warehouses=>Set<Warehouse>(); public DbSet<Stock> Stock=>Set<Stock>();
 public DbSet<Customer> Customers=>Set<Customer>(); public DbSet<Order> Orders=>Set<Order>(); public DbSet<OrderItem> OrderItems=>Set<OrderItem>();
 public DbSet<Delivery> Deliveries=>Set<Delivery>(); public DbSet<User> Users=>Set<User>(); public DbSet<AuditEvent> AuditEvents=>Set<AuditEvent>();
 protected override void OnModelCreating(ModelBuilder b){
  b.Entity<Product>().HasIndex(x=>x.Sku).IsUnique(); b.Entity<Product>().Property(x=>x.Price).HasPrecision(18,2);
  b.Entity<Order>().Property(x=>x.TotalAmount).HasPrecision(18,2); b.Entity<OrderItem>().Property(x=>x.UnitPrice).HasPrecision(18,2);
  b.Entity<Stock>().ToTable("Stock"); b.Entity<AuditEvent>().ToTable("AuditEvent");
 }}
