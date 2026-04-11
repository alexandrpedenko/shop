# Tech Stack

## Runtime
- **.NET 8** / **ASP.NET Core 8**
- Primary language: **C# 12** (primary constructors, `record struct`, collection expressions)

## Data Layer
| Concern | Technology |
|---|---|
| Primary ORM | Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Bulk operations | RepoDb + `RepoDb.SqlServer.BulkOperations` (used only for CSV bulk price merge) |
| Database | Microsoft SQL Server 2019 (Docker container) |
| DbContext | `ShopContext : IdentityDbContext<IdentityUser>` |
| Migrations | EF Core CLI migrations in `Shop.Core/Migrations/` |

## Cache & Messaging
- **Redis** via `StackExchange.Redis 2.8.22`
- Singleton `IConnectionMultiplexer`, `RedisPublisher` wraps `GetSubscriber().PublishAsync`
- Channel: `order_channel` (pub/sub between API and Warehouse.Logger)

## Auth & Identity
- **ASP.NET Core Identity** (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`)
- **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`) — HMAC-SHA256, 1h expiry
- Roles seeded at startup: `Admin`, `Customer`

## API Layer
- **Swashbuckle 6** (Swagger UI + XML comments + Bearer security definition)
- **Asp.Versioning.Http 8** — URL segment versioning (`/api/v{version}/...`)
- AutoModel binding + Data Annotations on `Contracts/` DTOs

## Application Layer
- **AutoMapper 13** (`ProductProfile` — `Product ↔ ProductModel`)
- **FluentValidation 11** — validation exception caught by `ErrorHandlingMiddleware`
- **CsvHelper 33** — CSV parsing for bulk product price update

## Logging
- **Serilog 9** (`Console` + `File` sinks, compact JSON, daily rolling, 30-day retention)
- `Microsoft.*` / `System.*` namespaces overridden to `Error` in config

## Infrastructure / DevOps
- **Docker Compose** — `mssql` (port 1433) + `redis` (port 6379) with named volumes
- Config: `appsettings.json` + env-variable overrides; secrets injected via environment

## Testing
| Tool | Usage |
|---|---|
| xUnit 2.5 | Test runner |
| FluentAssertions 6 | Assertion DSL |
| `WebApplicationFactory<Program>` | Integration test host |
| Real SQL Server (`TestDB`) | Integration tests — NOT in-memory |
| coverlet.collector | Code coverage |

## Notable Package Choices
- `RepoDb` is used **only** for the CSV bulk merge path (not a general repository). EF Core is the default data access.
- Dual-ORM is intentional: EF for transactional CRUD, RepoDb for high-throughput bulk operations.
