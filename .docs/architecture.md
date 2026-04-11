# Architecture

## Overview
Three-tier **Clean Architecture** with DDD tactical patterns. Dependency direction: `API → Core → Domain`. Domain has **zero** external dependencies.

```
Shop.API            Presentation layer  — controllers, contracts (req/res DTOs), DI composition root
Shop.Core           Application + Infrastructure — services, EF DbContext, helpers, middlewares, validators, DTOs, exceptions, AutoMapper
Shop.Domain         Domain layer — entities, value objects, domain services (NO external packages)
Shop.Warehouse.Logger  Standalone microservice — Redis subscriber, logs order events
test/               xUnit integration + unit tests
```

## Layer Responsibilities

### Shop.Domain
- **Entities:** `Product` (`record struct`), `Order` (`sealed class`), `OrderLine` (`readonly record struct`), `User` (unused — stub)
- **Value Objects:** `Price`, `SKU`, `Title`, `Description`, `Quantity`, `DiscountPercentage` — all `readonly record struct` with constructor-enforced invariants and `implicit operator` conversions
- **Domain Services:** `OrderDomainService` (factory, validates non-empty lines), `OrderDiscountService` (static, delegates to `Order.ApplyDiscountPercentage`)
- No repositories, no EF, no I/O

### Shop.Core
- **EF Models:** `ProductModel`, `OrderModel`, `OrderLineModel` — separate from domain entities, mapped by AutoMapper
- **Entity Mappings:** `IEntityTypeConfiguration<T>` implementations in `DataEF/EntityMappings/` (⚠️ `OrderMapping` + `OrderLineMapping` exist but are NOT registered in `OnModelCreating` — EF uses conventions for those tables)
- **Services:** `ProductsService`, `OrdersService`, `RedisPublisher` — all `sealed`, constructor-injected, return `OperationResult<T>`
- **Helpers:** `JwtTokenHelper` (token generation), `OperationResult<T>` + `OperationResultExtensions.CheckForAction()` (error transport)
- **Middlewares:** `ErrorHandlingMiddleware` (global exception → HTTP response), `AuthorizationLoggerMiddleware` (logs 401/403)
- **Exceptions:** `ApiException` hierarchy (`NotFoundException`, `BadRequestException`, `UnauthorizedException`, `InternalServerErrorException`), plus domain-specific `ProductNotFoundException`
- **Validators:** `ValidateIdAttribute` (global `ActionFilterAttribute` — rejects `id ≤ 0`)
- **Mapping:** `ProductProfile` (AutoMapper — `Product ↔ ProductModel`)

### Shop.API
- **Controllers:** `AuthController`, `ProductsController` (Admin), `OrdersController` (Customer)
- **Contracts:** `Requests/` and `Responses/` DTOs — Data Annotations validated
- **Extensions:** `ServicesExtensions.InitServices()` (all DI registrations), `AppExtensions.InitAppConfigAsync()` (role seeding, middleware, routing)

## Key Patterns

### OperationResult Pattern
Services return `OperationResult<T>` instead of throwing for expected failures. Controllers call `.CheckForAction()` which maps error types to HTTP status codes:
```
ErrorType.NotFound    → 404
ErrorType.Validation  → 400
ErrorType.Unexpected  → 500
```

### Value Objects as Domain Guard Rails
All domain primitives validate invariants at construction. `implicit operator` enables clean usage:
```csharp
Price price = 42.0m;   // throws if ≤ 0
SKU sku = "ABC-001";   // throws if empty or > 50 chars
```

### Dual ORM Strategy
- **EF Core** — default for all CRUD and transactional operations
- **RepoDb** `BulkMergeAsync` — used exclusively for CSV-driven bulk product price updates (merge on `SKU` qualifier)

### Redis Pub/Sub
After successful order creation, API publishes `{ OrderId, Timestamp }` JSON to `order_channel`. `Shop.Warehouse.Logger` subscribes and logs the event. This is a one-way event fanout (no reply).

## Data Model

```
Product (AlternateKey: SKU)
  └── OrderLine (FK: ProductSKU → Product.SKU, Restrict delete)
        └── Order (FK: OrderId → Order.Id, Cascade delete)
```

**Order TotalPrice** = `∑ (line.Price × line.Quantity)` — computed at creation, reduced by discount.

## Auth Flow
```
POST /api/auth/register → Identity creates user + assigns role
POST /api/auth/login    → Identity verifies password → JwtTokenHelper issues JWT
Bearer token            → JWT middleware validates → role claims extracted → [Authorize(Roles=...)]
```

## Middleware Pipeline Order
```
1. Inline header logger (console only)
2. ErrorHandlingMiddleware (global exception handler)
3. AuthorizationLoggerMiddleware (logs 401/403)
4. UseHttpsRedirection
5. UseAuthentication
6. UseAuthorization
7. MapControllers
```

## Known Issues / Tech Debt
- `OrderMapping` + `OrderLineMapping` not applied in `OnModelCreating` (EF uses conventions)
- `Shop.Domain/Users/User.cs` is unused — Identity's `IdentityUser` is used instead
- `JwtSettings:SecretKey = "my"` in base `appsettings.json` — must override in all environments
- `RedisPublisher.PublishAsync` is awaited inside the order service — Redis failure will fail the order request
- No repository abstraction — `ShopContext` injected directly into services
- No CQRS — services handle both reads and writes
