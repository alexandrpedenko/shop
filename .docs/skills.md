# Agent Skills Reference

Quick-reference for common tasks in this repository. Use this to know which file to edit, which pattern to follow, and which pitfalls to avoid.

---

## Add a New Domain Entity

1. Create `Shop.Domain/{Entity}/{Entity}.cs` — use `sealed class` for aggregate roots, `readonly record struct` for value objects or simple data carriers.
2. Add related value objects to `Shop.Domain/Common/` if reusable, or inline as nested `readonly record struct`.
3. Create `Shop.Core/DataEF/Models/{Entity}Model.cs` — plain POCO, no logic.
4. Create `Shop.Core/DataEF/EntityMappings/{Entity}Mapping.cs` implementing `IEntityTypeConfiguration<{Entity}Model>`.
5. Register the mapping in `ShopContext.OnModelCreating`: `modelBuilder.ApplyConfiguration(new {Entity}Mapping())`.
6. Add `DbSet<{Entity}Model>` to `ShopContext`.
7. Run migration: `dotnet ef migrations add Add{Entity} --project Shop.Core --startup-project Shop.API`.
8. Add AutoMapper profile in `Shop.Core/Mapping/{Entity}Profile/{Entity}Profile.cs` and register in `ServicesExtensions`.

---

## Add a New API Endpoint

1. If new controller: create `Shop.API/Controllers/{Feature}Controller.cs` — `sealed`, primary constructor, `[ApiController]`, `[Route("api/v{version:apiVersion}/...")]`, `[ApiVersion("1")]`, `[Authorize(Roles = "...")]`.
2. Add request DTO to `Shop.API/Contracts/Requests/{Action}{Feature}RequestDto.cs` with Data Annotations.
3. Add response DTO to `Shop.API/Contracts/Responses/{Action}{Feature}ResponseDto.cs`.
4. Implement service method in `Shop.Core/Services/{Feature}/{Feature}Service.cs` returning `OperationResult<T>`.
5. In controller, call service → call `.CheckForAction()` → return `Created`/`Ok` on success.

---

## Add a New Exception Type

- Extend `ApiException` in `Shop.Core/Exceptions/`:
  ```csharp
  public sealed class {Entity}NotFoundException(int id)
      : NotFoundException($"{Entity} with id: {id} not found");
  ```
- `ErrorHandlingMiddleware` will automatically catch it and map to the correct HTTP status.

---

## Add a New Value Object

In `Shop.Domain/Common/`:
```csharp
public readonly record struct MyValue
{
    public readonly SomeType Value;
    public MyValue(SomeType value)
    {
        // validate — throw ArgumentException if invalid
        Value = value;
    }
    public static implicit operator MyValue(SomeType value) => new(value);
    public static implicit operator SomeType(MyValue vo) => vo.Value;
}
```

---

## Bulk Data Operation

Use RepoDb pattern from `ProductsService.BulkUpdateAsync`:
1. Map domain/DTO list to `*Model` list.
2. Open raw `SqlConnection` from `_options.Value.ConnectionString`.
3. Call `await connection.BulkMergeAsync<{Model}>(items, qualifiers: [e => e.SKU])`.
4. Do NOT use EF `SaveChangesAsync` for bulk operations.

---

## Add a Redis Event

1. Inject `RedisPublisher` into the service.
2. After successful persistence, call `await _redisPublisher.PublishAsync("channel_name", jsonPayload)`.
3. If `Shop.Warehouse.Logger` needs to handle the event, subscribe to the channel in its `Program.cs`.

---

## Auth — Add a New Role

1. Add the role name to the `RegisterRequestDto.Role` regex: `[RegularExpression("Admin|Customer|NewRole")]`.
2. Add seeding in `AppExtensions.InitRolesAsync`: `await roleManager.CreateAsync(new IdentityRole("NewRole"))`.
3. Apply `[Authorize(Roles = "NewRole")]` to the relevant controller or action.

---

## Key File Locations

| What | Where |
|---|---|
| DI registration | `Shop.API/Extensions/ServicesExtensions.cs` |
| App pipeline / startup | `Shop.API/Extensions/AppExtensions.cs` |
| DbContext | `Shop.Core/DataEF/DbContext.cs` |
| EF entity configs | `Shop.Core/DataEF/EntityMappings/` |
| EF data models | `Shop.Core/DataEF/Models/` |
| Domain entities | `Shop.Domain/{Feature}/` |
| Value objects | `Shop.Domain/Common/` |
| Domain services | `Shop.Domain/Services/` |
| Application services | `Shop.Core/Services/{Feature}/` |
| JWT helper | `Shop.Core/Helpers/JWT/JwtTokenHelper.cs` |
| OperationResult | `Shop.Core/Helpers/OperationResult/` |
| Exception hierarchy | `Shop.Core/Exceptions/` |
| Middlewares | `Shop.Core/Middlewares/` |
| AutoMapper profiles | `Shop.Core/Mapping/` |
| API controllers | `Shop.API/Controllers/` |
| Request DTOs | `Shop.API/Contracts/Requests/` |
| Response DTOs | `Shop.API/Contracts/Responses/` |
| Integration tests | `test/Shop.API.IntegrationTests/ApiIntegrationTests/` |
| Domain unit tests | `test/Shop.API.IntegrationTests/DomainUnitTests/` |
| Test factory | `test/Shop.API.IntegrationTests/CustomWebApplicationFactory.cs` |
| Docker services | `Shop.API/docker-compose.yml` |
| Config | `Shop.API/appsettings.json` |

---

## Common Pitfalls

| Pitfall | Correct Approach |
|---|---|
| Adding logic to EF `*Model` classes | Keep `*Model` as plain data containers; put logic in domain entities |
| Adding NuGet packages to `Shop.Domain` | Domain must stay dependency-free |
| Forgetting to register new `IEntityTypeConfiguration` in `OnModelCreating` | Always call `modelBuilder.ApplyConfiguration(new XMapping())` |
| Throwing exceptions in services for expected failures | Return `OperationResult<T>.Failure(errorType, message)` |
| Using in-memory DB in integration tests | Tests use real SQL Server (`TestDB`) — check `appsettings.Test.json` |
| Committing real `JwtSettings:SecretKey` | Use env vars / user secrets; base config has placeholder `"my"` |
