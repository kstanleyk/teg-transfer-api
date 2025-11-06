using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TegWallet.Application.Interfaces.Auth;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Photos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Infrastructure.Persistence.Context;
using TegWallet.Infrastructure.Persistence.Repository;
using TegWallet.Infrastructure.Persistence.Repository.Auth;
using TegWallet.Infrastructure.Persistence.Repository.Core;
using TegWallet.Infrastructure.Photos;

namespace TegWallet.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<TegWalletContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Data"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<TegWalletDatabaseSeeder>();

        services.AddIdentity<Client, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<TegWalletContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        services.Configure<CloudinarySettings>(configuration
            .GetSection("CloudinarySettings"));

        services.AddScoped<IPhotoService, PhotoService>();

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Core
        //services.AddScoped<IClientRepository, UserManager>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IClientGroupRepository, ClientGroupRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IExchangeRateHistoryRepository, ExchangeRateHistoryRepository>();

        return services;
    }

    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var seeders = serviceScope.ServiceProvider.GetServices<TegWalletDatabaseSeeder>();

        foreach (var seeder in seeders) seeder.SeedDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }
}