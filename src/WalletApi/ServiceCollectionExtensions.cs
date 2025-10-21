using System.Globalization;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using TegWallet.Application.Interfaces.Localization;
using TegWallet.WalletApi.Localization;
using TegWallet.WalletApi.Services;

namespace TegWallet.WalletApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT settings
        var jwtKey = configuration["Settings:Key"]!;
        var jwtIssuer = configuration["Settings:Issuer"]!;

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
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddAuthorization();

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
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();

            config.Filters.Add(new AuthorizeFilter(policy));
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<IAppLocalizer, AppLocalizer>();

        return services;
    }

    public static WebApplication UseLocalizationServices(this WebApplication app)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en"),
            new CultureInfo("fr")
        };

        var queryProvider = new QueryStringRequestCultureProvider
        {
            QueryStringKey = "lang",
            UIQueryStringKey = "lang"
        };

        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders = new List<IRequestCultureProvider>
            {
                queryProvider,
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            }
        };

        app.UseRequestLocalization(localizationOptions);

        return app;
    }
}

internal class WebProtocolSettings
{
    public required string Url { get; set; }
    public int Port { get; set; }
}