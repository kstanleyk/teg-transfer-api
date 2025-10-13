using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using TegWallet.Application.Authorization;
using TegWallet.WebApi.Permissions;
using TegWallet.WebApi.Services;

namespace TegWallet.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authorizationServer = configuration["AppSettings:AuthorizationServer"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authorizationServer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                };
                options.RequireHttpsMetadata = false;

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine($"Auth failed: {ctx.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = _ =>
                    {
                        Console.WriteLine("Token validated successfully!");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var prop in typeof(AppPermissions).GetNestedTypes().SelectMany(c =>
                         c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = prop.GetValue(null);
                if (propertyValue is not null)
                {
                    options.AddPolicy(propertyValue.ToString()!, policy => policy.RequireClaim(AppClaim.Permission, propertyValue.ToString()!));
                }
            }
        });

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services,
        IConfiguration configuration, ConfigureWebHostBuilder host, string allowedCorsOrigins)
    {
        var protocolSettings = configuration.GetSection("WebProtocolSettings").Get<WebProtocolSettings>();
        if (protocolSettings != null)
        {
            host.UseUrls($"{protocolSettings.Url}:{protocolSettings.Port}");
        }

        services
            .AddOptions<WebProtocolSettings>()
            .Bind(configuration.GetSection("WebProtocolSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(allowedCorsOrigins, corsPolicy =>
            {
                corsPolicy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddControllersWithViews(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            config.Filters.Add(new AuthorizeFilter(policy));
        });


        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<TokenInfoService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // Adds headers: api-supported-versions, api-deprecated-versions
                // You can also set ApiVersionReader (see next step)

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),               // /api/v1/controller
                    new QueryStringApiVersionReader("api-version") // /api/controller?api-version=1.0
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // e.g. "v1"
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}

internal class WebProtocolSettings
{
    public required string Url { get; set; }
    public int Port { get; set; }
}