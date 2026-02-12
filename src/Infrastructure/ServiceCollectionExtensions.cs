using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TegWallet.Application.Interfaces.Auth;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Application.Interfaces.Kyc;
using TegWallet.Application.Interfaces.Photos;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Infrastructure.Persistence.Context;
using TegWallet.Infrastructure.Persistence.Repository;
using TegWallet.Infrastructure.Persistence.Repository.Auth;
using TegWallet.Infrastructure.Persistence.Repository.Core;
using TegWallet.Infrastructure.Persistence.Repository.Kyc;
using TegWallet.Infrastructure.Photos;

namespace TegWallet.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<TegWalletContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Data"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<TegWalletDatabaseSeeder>();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<TegWalletContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        //services.Configure<CloudinarySettings>(
        //    configuration.GetSection("CloudinarySettings"));

        //services.AddScoped<IDocumentService, DocumentService>();

        // Register Cloudinary settings
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

        // Register DocumentService with logging
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
            return new Cloudinary(account);
        });

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Core
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IClientGroupRepository, ClientGroupRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<IRateLockRepository, RateLockRepository>();
        services.AddScoped<IExchangeRateTierRepository, ExchangeRateTierRepository>();
        services.AddScoped<IMinimumAmountConfigurationRepository, MinimumAmountConfigurationRepository>();
        services.AddScoped<IDocumentAttachmentRepository, DocumentAttachmentRepository>();
        services.AddScoped<IKycProfileRepository, KycProfileRepository>();

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