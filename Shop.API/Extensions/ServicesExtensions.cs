using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepoDb;
using Serilog;
using Shop.Core.DataEF;
using Shop.Core.Mapping.ProductProfile;
using Shop.Core.Services.Orders;
using Shop.Core.Services.Products;
using Shop.Core.Services.Redis;
using Shop.Core.Validators.Attributes;
using StackExchange.Redis;
using System.Reflection;
using System.Text;

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
            GlobalConfiguration
                .Setup()
                .UseSqlServer();

            AddSerilog(services, config);
            AddRedis(services, config);
            AddSwagger(services);
            AddApiVersioning(services);
            AddDbContext(services, config);
            AddMapper(services);
            AddServices(services);
            AddIdentity(services);
            AddAuthenticationWithJwt(services, config);

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
            services.AddTransient<RedisPublisher>();
            services.AddTransient<ProductService>();
            services.AddTransient<OrdersService>();

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, ConfigurationManager config)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect($"{config["Redis:Host"]}:{config["Redis:Port"]}"));

            return services;
        }

        public static IServiceCollection AddSerilog(this IServiceCollection services, ConfigurationManager config)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

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

                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Enter 'Bearer' [space] and your token",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                });

                options.AddSecurityRequirement(new()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
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

        private static void AddIdentity(IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ShopContext>();
        }

        private static void AddAuthenticationWithJwt(IServiceCollection services, ConfigurationManager config)
        {
            var jwtSettings = config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT secret key is not configured in appsettings.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }
    }
}
