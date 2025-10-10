using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transfer.Application.Interfaces.Auth;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Application.Interfaces.Sales;
using Transfer.Infrastructure.Persistence.Context;
using Transfer.Infrastructure.Persistence.Repository;
using Transfer.Infrastructure.Persistence.Repository.Auth;
using Transfer.Infrastructure.Persistence.Repository.Inventory;
using Transfer.Infrastructure.Persistence.Repository.Sales;

namespace Transfer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<AgrovetContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Data"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<AgrovetDatabaseSeeder>();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Inventory
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductMovementRepository, ProductMovementRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IOrderTypeRepository, OrderTypeRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
        services.AddScoped<IDistributionChannelRepository, DistributionChannelRepository>();
        services.AddScoped<IPriceItemRepository, PriceItemRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        return services;
    }

    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var seeders = serviceScope.ServiceProvider.GetServices<AgrovetDatabaseSeeder>();

        foreach (var seeder in seeders) seeder.SeedDatabaseAsync().GetAwaiter().GetResult();

        return app;
    }
}