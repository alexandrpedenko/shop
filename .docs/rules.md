# Coding Rules & Conventions

## Project Structure
- **Domain** (`Shop.Domain`) must remain dependency-free. Never add NuGet packages to it.
- **Core** (`Shop.Core`) owns all infrastructure: EF models, DbContext, services, helpers, middlewares.
- **API** (`Shop.API`) owns only: controllers, request/response contracts, DI wiring, startup extensions.
- Cross-project coupling flows only inward: `API → Core → Domain`.

## Naming
| Artifact | Convention | Example |
|---|---|---|
| EF Data Models | `*Model` suffix | `ProductModel`, `OrderModel` |
| Request DTOs | `*RequestDto` | `CreateProductRequestDto` |
| Response DTOs | `*ResponseDto` | `GetProductResponseDto` |
| Exception classes | Descriptive + `Exception` | `ProductNotFoundException` |
| Services | `*Service` | `ProductsService`, `OrdersService` |
| Domain entities | Plain noun | `Product`, `Order`, `OrderLine` |
| Value objects | Plain noun | `Price`, `SKU`, `Title` |

## C# Idioms
- **Prefer `sealed`** on all service, controller, helper, and DTO classes — prevents unintended inheritance.
- **Use primary constructors** for DI injection in services, controllers, and middlewares (C# 12 pattern used throughout).
- **Domain entities must use `readonly record struct`** (value objects) or `sealed class` (aggregate roots).
- **Value objects always use `readonly record struct`** with constructor-enforced invariants and `implicit operator` conversions. Never validate via attributes in the domain layer.
- Don't use `var` on the left side when the type is not immediately obvious from the right-hand side.

## Services
- Services return `OperationResult<T>` for expected failures (not found, validation errors). Never throw for expected cases.
- Use `OperationResult<T>.Failure(ErrorType, message)` for failures; `OperationResult<T>.Success(value)` for success.
- Controllers call `.CheckForAction()` on `OperationResult<T>` to convert to `IActionResult?` — if non-null, return early.
- Inject `ShopContext` directly (no repository abstraction). EF `DbSet<T>` is the data access mechanism.
- Use EF Core for all standard CRUD. Use RepoDb `BulkMergeAsync` only for bulk operations.

## Error Handling
- Define application errors as `ApiException` subclasses in `Shop.Core/Exceptions/`.
- Domain errors (invalid construction) throw `ArgumentException` / `ArgumentNullException` directly.
- `ErrorHandlingMiddleware` is the single catch-all — do not add try/catch inside controllers.
- FluentValidation exceptions are caught by middleware and return 400 with field-level errors.

## Validation
- **Request DTOs** (API layer): use Data Annotations (`[Required]`, `[EmailAddress]`, `[RegularExpression]`, etc.).
- **Domain objects**: validate invariants in constructors using `ArgumentException` for invalid state.
- **Id route params**: the global `ValidateIdAttribute` filter rejects `id ≤ 0` — no need to repeat this check in controllers.
- FluentValidation is available for complex business rule validation in `Shop.Core/Validators/`.

## Data / EF
- Always define `IEntityTypeConfiguration<T>` for each entity in `Shop.Core/DataEF/EntityMappings/` and register it in `ShopContext.OnModelCreating`.
- Do not put business logic in EF models (`*Model` classes are plain data containers).
- `ShopContext` is `Scoped` lifetime. `IConnectionMultiplexer` is `Singleton`.
- Run migrations via EF CLI: `dotnet ef migrations add <Name> --project Shop.Core --startup-project Shop.API`.

## Auth
- `ProductsController` → `[Authorize(Roles = "Admin")]`
- `OrdersController` → `[Authorize(Roles = "Customer")]`
- `AuthController` → no `[Authorize]`
- JWT secret must be set via environment variable / secrets — never commit a real key in `appsettings.json`.
- Roles: only `Admin` and `Customer` — role names are validated at registration (regex `Admin|Customer`).

## Mapping
- AutoMapper (`ProductProfile`) only for `Product ↔ ProductModel`. For new entity types, add a dedicated `*Profile` class.
- Manual mapping (no AutoMapper) is acceptable for DTOs that have a 1:1 field correspondence with simple types.
- Order-related mapping is done manually in `OrdersService` — keep this consistent.

## API Versioning
- All business endpoints are versioned: `/api/v{version}/...`.
- Auth endpoints are unversioned: `/api/auth/...`.
- Add new API versions by registering them in `ServicesExtensions` and using `[ApiVersion("N")]` on controllers.

## Logging
- Use Serilog structured logging (`Log.Information(...)`, `Log.Warning(...)`, etc.).
- Log significant domain events (order created, discount applied, auth failures).
- Do not log sensitive data (passwords, JWT secrets, PII).

## Testing
- Integration tests use `CustomWebApplicationFactory<Program>` with a real SQL Server (`TestDB`).
- Domain unit tests have zero I/O dependencies — test `OrderDomainService`, `OrderDiscountService`, and value objects directly.
- Use `FluentAssertions` for all assertions. Use `TestHelper.ShouldFail().WithErrors(...)` for HTTP failure assertions.
- `Program` must remain `public partial class` to allow `WebApplicationFactory<Program>`.
