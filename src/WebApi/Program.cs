using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using Agrovet.Application;
using Agrovet.Application.Authorization;
using Agrovet.Infrastructure;
using Agrovet.WebApi.Permissions;
using Agrovet.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var host = builder.WebHost;

// Configure settings without building service provider
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

services.AddApplication();
services.AddInfrastructure(configuration);
services.AddControllersWithViews(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    config.Filters.Add(new AuthorizeFilter(policy));
});

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

services.AddHttpContextAccessor();
services.AddScoped<CurrentUserService>();
services.AddScoped<TokenInfoService>();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.SeedDatabase();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();



internal class WebProtocolSettings
{
    public required string Url { get; set; }
    public int Port { get; set; }
}