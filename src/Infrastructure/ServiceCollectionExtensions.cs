using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TegWallet.Application.Interfaces.Auth;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Infrastructure.Persistence.Context;
using TegWallet.Infrastructure.Persistence.Repository;
using TegWallet.Infrastructure.Persistence.Repository.Auth;
using TegWallet.Infrastructure.Persistence.Repository.Core;

namespace TegWallet.Infrastructure;

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
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();

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