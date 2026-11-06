using System.Text; using Microsoft.AspNetCore.Authentication.JwtBearer; using Microsoft.AspNetCore.Diagnostics; using Microsoft.EntityFrameworkCore; using Microsoft.IdentityModel.Tokens;
using Logistika360.Api.Controllers; using Logistika360.Api.Data; using Logistika360.Api.Services;
var b=WebApplication.CreateBuilder(args); var c=b.Configuration;
// Secrets come from env vars / user-secrets: ConnectionStrings__Default, Jwt__Key (>=32 chars)
b.Services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(c.GetConnectionString("Default")));
b.Services.AddScoped<OrderService>().AddScoped<TokenService>().AddControllers();
b.Services.AddEndpointsApiExplorer().AddSwaggerGen(o=>{o.AddSecurityDefinition("Bearer",new(){Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http,Scheme="bearer"});
 o.AddSecurityRequirement(new(){{new(){Reference=new(){Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id="Bearer"}},new string[0]}});});
b.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new(){ValidIssuer=c["Jwt:Issuer"],ValidateAudience=false,
 IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(c["Jwt:Key"]??throw new("Jwt:Key not configured")))});
b.Services.AddAuthorization().AddCors(o=>o.AddDefaultPolicy(p=>p.WithOrigins(c["Cors:Origin"]!).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("X-Correlation-Id")));
var app=b.Build();
app.Use(async(ctx,next)=>{ var id=ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault()??Guid.NewGuid().ToString("N"); ctx.Response.Headers["X-Correlation-Id"]=id; ctx.Items["cid"]=id; await next();});
app.UseExceptionHandler(e=>e.Run(async ctx=>{ var ex=ctx.Features.Get<IExceptionHandlerFeature>()?.Error; var code=ex is AppException a?a.Code:ex is DbUpdateConcurrencyException?409:500;
 ctx.Response.StatusCode=code; await ctx.Response.WriteAsJsonAsync(new{error=code==500?"Unexpected error.":ex!.Message,correlationId=ctx.Items["cid"]});}));
app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.UseSwagger(); app.UseSwaggerUI(); app.MapControllers(); app.Run();
