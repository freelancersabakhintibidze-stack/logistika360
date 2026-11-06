# Logistika 360
ASP.NET Core 8 API (`backend/`), Angular 17 UI (`frontend/`), analytics SQL (`sql/analytics.sql`). Uses your existing SQL Server schema (no DB scripts).
## Run
Backend (secrets via env, never committed):
    cd backend
    export ConnectionStrings__Default="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=true"
    export Jwt__Key="<random string, 32+ chars>"
    dotnet run --urls http://localhost:5000      # Swagger at /swagger
Frontend: `cd frontend && npm install && npm start` (API base URL in `src/app/core/auth.service.ts`).
## Notes
- Users.PasswordHash must be BCrypt. Entity/column names follow `Models/Entities.cs`; adjust mappings in `AppDbContext` if your schema differs.
- Roles enforced server-side (403). Order transitions are role-gated; stock is reserved on Submit in a Serializable transaction + RowVersion (409 on conflict) and consumed on Delivered.
- Errors return `{error, correlationId}` plus an `X-Correlation-Id` header.
## Not included yet
UI create/edit forms for products and orders, user admin screen, automated tests, ER diagram, demo video, auto-creating deliveries at Dispatch.
