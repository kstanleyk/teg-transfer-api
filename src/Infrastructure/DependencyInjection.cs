using Agrovet.Application.Interfaces.Auth;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Infrastructure.Persistence.Context;
using Agrovet.Infrastructure.Persistence.Repository;
using Agrovet.Infrastructure.Persistence.Repository.Auth;
using Agrovet.Infrastructure.Persistence.Repository.Core;
using Agrovet.Infrastructure.Persistence.Repository.Inventory;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agrovet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework
        services.AddDbContext<AgrovetContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Data"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<AgrovetDatabaseSeeder>();

        services.AddScoped<IDatabaseFactory, DatabaseFactory>();

        // Repositories

        //Auth
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

        //Core
        services.AddScoped<IAverageWeightRepository, AverageWeightRepository>();
        services.AddScoped<IEstateRepository, EstateRepository>();

        //Inventory
        services.AddScoped<IItemCategoryRepository, ItemCategoryRepository>();
        services.AddScoped<IItemMovementRepository, ItemMovementRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

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