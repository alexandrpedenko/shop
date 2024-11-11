using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF;
using Shop.Core.Mapping.ProductProfile;
using Shop.Core.Services.Orders;
using Shop.Core.Services.Products;
using Shop.Core.Validators.Attributes;
using System.Reflection;

namespace Shop.API.Extensions
{
    /// <summary>
    /// Init app services extensions
    /// </summary>
    public static class ServicesExtensions
    {
        /// <summary>
        /// Services configuration
        /// </summary>
        public static IServiceCollection InitServices(this IServiceCollection services, ConfigurationManager config)
        {
            AddSwagger(services);
            AddApiVersioning(services);
            AddDbContext(services, config);
            AddMapper(services);
            AddServices(services);

            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateIdAttribute>();
            });
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        private static void AddDbContext(IServiceCollection services, ConfigurationManager config)
        {
            services.AddDbContext<ShopContext>(optionsBuilder =>
            {
                optionsBuilder
                    .UseSqlServer(config.GetConnectionString("DefaultConnection"))
                    .LogTo(Console.WriteLine);
            },
            ServiceLifetime.Scoped,
            ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                config.AddProfile(new ProductProfile());
            });

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<ProductService>();
            services.AddTransient<OrdersService>();

            return services;
        }

        private static void AddSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "Shop.Api",
                    Version = "v1"
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        private static void AddApiVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1);
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader());
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        }
    }
}
