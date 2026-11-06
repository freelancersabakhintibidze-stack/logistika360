using System.ComponentModel.DataAnnotations;
namespace Logistika360.Api.Models;
public static class Roles { public const string Admin="Admin", Manager="WarehouseManager", Operator="Operator", Courier="Courier", Viewer="Viewer"; }
public class Product { public int Id{get;set;} [Required] public string Sku{get;set;}=""; [Required,MaxLength(150)] public string Name{get;set;}=""; public string? Category{get;set;} [Range(0,double.MaxValue)] public decimal Price{get;set;} public bool IsActive{get;set;}=true; }
public class Warehouse { public int Id{get;set;} public string Name{get;set;}=""; }
public class Stock { public int Id{get;set;} public int ProductId{get;set;} public int WarehouseId{get;set;} public int Quantity{get;set;} public int ReservedQuantity{get;set;} [Timestamp] public byte[]? RowVersion{get;set;} }
public class Customer { public int Id{get;set;} public string Name{get;set;}="";public string? Phone{get;set;} public string? Address{get;set;} }
public class Order { public int Id{get;set;} public int CustomerId{get;set;} public string Status{get;set;}="Draft"; public decimal TotalAmount{get;set;} public DateTime CreatedAt{get;set;}=DateTime.UtcNow; public string? CreatedBy{get;set;} public List<OrderItem> Items{get;set;}=new(); }
public class OrderItem { public int Id{get;set;} public int OrderId{get;set;} public int ProductId{get;set;} public int WarehouseId{get;set;} public int Quantity{get;set;} public decimal UnitPrice{get;set;} }
public class Delivery { public int Id{get;set;} public int OrderId{get;set;} public int? CourierId{get;set;} public string Status{get;set;}="Pending"; public DateTime? ScheduledAt{get;set;} public DateTime? CompletedAt{get;set;} public string? Address{get;set;} public string? Phone{get;set;} public string? FailReason{get;set;} }
public class User { public int Id{get;set;} public string Username{get;set;}=""; public string PasswordHash{get;set;}=""; public string Role{get;set;}=Roles.Viewer; }
public class AuditEvent { public int Id{get;set;} public DateTime At{get;set;}=DateTime.UtcNow; public string? UserName{get;set;} public string Entity{get;set;}=""; public string Action{get;set;}=""; public string? Details{get;set;} }
public record LoginDto(string Username,string Password);
public record OrderItemDto(int ProductId,int WarehouseId,int Quantity);
public record OrderCreateDto(int CustomerId,List<OrderItemDto> Items);
public record StatusDto(string Status);
public record DeliveryUpdateDto(string Status,string? FailReason,int? CourierId,DateTime? ScheduledAt);
