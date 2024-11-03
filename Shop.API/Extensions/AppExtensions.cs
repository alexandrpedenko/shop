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
        public static IApplicationBuilder InitAppConfig(this WebApplication app)
        {
            InitMiddlewares(app);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            return app;
        }

        private static void InitMiddlewares(WebApplication app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}