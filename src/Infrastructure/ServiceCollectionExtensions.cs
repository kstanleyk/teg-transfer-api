using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transfer.Application.Interfaces.Auth;
using Transfer.Application.Interfaces.Core;
using Transfer.Infrastructure.Persistence.Context;
using Transfer.Infrastructure.Persistence.Repository;
using Transfer.Infrastructure.Persistence.Repository.Auth;
using Transfer.Infrastructure.Persistence.Repository.Core;

namespace Transfer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<TransferContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Data"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<TransferDatabaseSeeder>();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Core
        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }

    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var seeders = serviceScope.ServiceProvider.GetServices<TransferDatabaseSeeder>();

        foreach (var seeder in seeders) seeder.SeedDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }
}