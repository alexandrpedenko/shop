using Microsoft.AspNetCore.Identity;
using Shop.Core.Middlewares;

namespace Shop.API.Extensions
{
    /// <summary>
    /// Init app config extensions
    /// </summary>
    public static class AppConfiguration
    {
        /// <summary>
        /// App configuration
        /// </summary>
        public static async Task<IApplicationBuilder> InitAppConfigAsync(this WebApplication app)
        {
            await InitRolesAsync(app);
            InitMiddlewares(app);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        private static void InitMiddlewares(WebApplication app)
        {
            app.Use(async (context, next) =>
            {
                var authHeader = context.Request.Headers["Authorization"];
                Console.WriteLine($"Authorization Header: {authHeader}");
                await next.Invoke();
            });

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<AuthorizationLoggerMiddleware>();
        }

        private static async Task InitRolesAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}