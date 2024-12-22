using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shop.Core.DataEF;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            services.AddSerilog(configure =>
            {
                configure.ReadFrom.Configuration(context.Configuration);
            });

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

            using var scope = services.BuildServiceProvider().CreateScope();
            var scopedServices = scope.ServiceProvider;
            var dbContext = scopedServices.GetRequiredService<ShopContext>();

            dbContext.Database.Migrate();

            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            InitializeRolesAsync(roleManager).GetAwaiter().GetResult();
        });
    }

    public async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
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

        context.Database.ExecuteSqlRaw("DELETE FROM AspNetUserRoles");
        context.Database.ExecuteSqlRaw("DELETE FROM AspNetUsers");
        context.Database.ExecuteSqlRaw("DELETE FROM AspNetRoles");

        context.SaveChanges();
    }

    public void SeedDatabase(Action<ShopContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShopContext>();

        seedAction(context);
        context.SaveChanges();
    }
}

[CollectionDefinition("Database Tests", DisableParallelization = true)]
public abstract class ApiTestsBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory<Program> _factory;

    protected ApiTestsBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected ShopContext GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ShopContext>();
    }

    protected void ClearTables()
    {
        _factory.ClearTables();
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() => _factory.ApplyMigrations());
        await Task.Run(() => ClearTables());
    }

    protected void InitializeDatabase(Action<ShopContext> seedAction)
    {
        ClearTables();
        _factory.SeedDatabase(seedAction);
    }

    protected void SeedUsersAndRoles(Action<UserManager<IdentityUser>, RoleManager<IdentityRole>> seedAction)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        seedAction(userManager, roleManager);
    }

    protected async Task AuthorizeAsCustomer()
    {
        var token = await GenerateJwtToken("customer@example.com", "Customer");

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task AuthorizeAdmin()
    {
        var token = await GenerateJwtToken("admin@example.com", "Admin");

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<string> GenerateJwtToken(string email, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await _factory.InitializeRolesAsync(roleManager);

        var user = new IdentityUser { UserName = email, Email = email };
        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, role);

        var jwtSettings = config.GetSection("JwtSettings");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(
                [
                    new(JwtRegisteredClaimNames.Sub, user.Id),
                    new(JwtRegisteredClaimNames.Email, user.Email),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Role, role),
                ]
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task DisposeAsync()
    {
        await Task.Run(() => ClearTables());
    }
}