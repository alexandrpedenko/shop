# Shop — Copilot Instructions

This is a **.NET 8 e-commerce REST API** with DDD tactical patterns, Clean Architecture, and a standalone Redis event consumer microservice.

## Projects at a Glance
| Project | Role |
|---|---|
| `Shop.Domain` | Domain entities, value objects, domain services — **zero external dependencies** |
| `Shop.Core` | Application + infrastructure — EF Core, services, helpers, middlewares, validators |
| `Shop.API` | Presentation — controllers, request/response DTOs, DI composition root |
| `Shop.Warehouse.Logger` | Standalone Redis pub/sub subscriber (logs order events) |
| `test/Shop.API.IntegrationTests` | xUnit integration tests + domain unit tests |

## Core Tech
- .NET 8 · ASP.NET Core 8 · C# 12
- EF Core 8 (SQL Server) + RepoDb (bulk ops only)
- ASP.NET Core Identity + JWT Bearer (HMAC-SHA256, 1h)
- Redis (`StackExchange.Redis`) pub/sub on channel `order_channel`
- AutoMapper 13 · FluentValidation 11 · Serilog 9 · CsvHelper 33
- Swagger + API versioning (`/api/v{version}/...`)
- xUnit + FluentAssertions + `WebApplicationFactory`

## Key Patterns
- **OperationResult\<T\>** — services return this, not exceptions. Controllers call `.CheckForAction()`.
- **Value objects** — `readonly record struct` with constructor invariants + `implicit operator`. In `Shop.Domain/Common/`.
- **EF models** (`*Model`) are separate from domain entities — mapped via AutoMapper in `Shop.Core/DataEF/Models/`.
- **`IEntityTypeConfiguration<T>`** in `Shop.Core/DataEF/EntityMappings/` — must be registered in `OnModelCreating`.
- **Global `ValidateIdAttribute`** rejects `id ≤ 0` — no need to repeat in controllers.
- **`ErrorHandlingMiddleware`** is the single exception catch-all — no try/catch in controllers.

## Auth
- `ProductsController` → `[Authorize(Roles = "Admin")]`
- `OrdersController` → `[Authorize(Roles = "Customer")]`
- `AuthController` → public (no auth)

## Dependency Direction
```
Shop.API → Shop.Core → Shop.Domain
```
Never add external packages to `Shop.Domain`. Never reference `Shop.API` from `Shop.Core`.

## Detailed Docs
All detailed reference docs are in `.docs/`:
- `.docs/tech-stack.md` — full package list and version info
- `.docs/architecture.md` — layer responsibilities, data model, middleware order, known issues
- `.docs/rules.md` — naming conventions, C# idioms, service patterns, validation, testing rules
- `.docs/skills.md` — step-by-step guides for common tasks (add entity, add endpoint, add exception, etc.)
