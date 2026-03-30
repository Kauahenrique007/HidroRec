using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using HidroRec.Backend.Application.Interfaces;
using HidroRec.Backend.Application.Services;
using HidroRec.Backend.Application.Validators;
using HidroRec.Backend.Domain.Interfaces;
using HidroRec.Backend.Infrastructure.Data;
using HidroRec.Backend.Infrastructure.ExternalServices;
using HidroRec.Backend.Infrastructure.Repositories;
using HidroRec.Backend.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

namespace HidroRec.Backend.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHidroRecOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<ExternalDataOptions>(configuration.GetSection(ExternalDataOptions.SectionName));
        return services;
    }

    public static IServiceCollection AddHidroRecData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HidroRecDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IReporteRepository, ReporteRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        return services;
    }

    public static IServiceCollection AddHidroRecSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrador"));
            options.AddPolicy("GestorOuAdmin", policy => policy.RequireRole("Administrador", "Gestor"));
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddHidroRecApplication(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("AuthPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        services.AddCors(options =>
        {
            options.AddPolicy("FrontendDevelopment", policy =>
            {
                policy
                    .SetIsOriginAllowed(origin =>
                    {
                        if (string.Equals(origin, "null", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                        {
                            return false;
                        }

                        return uri.Host is "localhost" or "127.0.0.1";
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddEndpointsApiExplorer();
        services.AddAutoMapper(typeof(MappingProfile));
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateReporteRequestDtoValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IPrevisaoService, PrevisaoService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddHttpClient<WeatherService>();
        services.AddScoped<IWeatherService>(provider => provider.GetRequiredService<WeatherService>());
        services.AddScoped<ITideService, TideService>();

        return services;
    }

    public static IServiceCollection AddHidroRecSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HidroRec API",
                Version = "v1",
                Description = "API do MVP HidroRec para monitoramento urbano inteligente."
            });

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Description = "Informe o token JWT no formato Bearer {token}.",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [jwtSecurityScheme] = Array.Empty<string>()
            });
        });

        return services;
    }
}
