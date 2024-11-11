using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Core.DataEF;

namespace Shop.API.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var projectDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            var configPath = Path.Combine(projectDir, "appsettings.Test.json");
            config.AddJsonFile(configPath, optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ShopContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ShopContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });
    }

    public void ApplyMigrations()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopContext>();
        context.Database.Migrate();
    }

    public void ClearTables()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopContext>();

        context.Database.ExecuteSqlRaw("DELETE FROM OrderLines");
        context.Database.ExecuteSqlRaw("DELETE FROM Orders");
        context.Database.ExecuteSqlRaw("DELETE FROM Products");
    }

    public void SeedDatabase(Action<ShopContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopContext>();

        seedAction(context);
        context.SaveChanges();
    }
}

public abstract class ApiTestsBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory<Program> _factory;

    protected ApiTestsBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() => _factory.ApplyMigrations());
    }

    protected void InitializeDatabase(Action<ShopContext> seedAction)
    {
        _factory.SeedDatabase(seedAction);
    }

    public async Task DisposeAsync()
    {
        await Task.Run(() => _factory.ClearTables());
    }
}