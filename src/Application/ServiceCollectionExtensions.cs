using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Transfer.Application.Features.Inventory.Category;
using Transfer.Application.Features.Inventory.Order;
using Transfer.Application.Features.Inventory.OrderDetail;
using Transfer.Application.Features.Inventory.OrderStatus;
using Transfer.Application.Features.Inventory.OrderType;
using Transfer.Application.Features.Inventory.Product;
using Transfer.Application.Features.Inventory.ProductMovement;
using Transfer.Application.Features.Inventory.Supplier;
using Transfer.Application.Features.Inventory.Warehouse;
using Transfer.Application.Features.Sales.DistributionChannel;
using Transfer.Application.Features.Sales.PriceItem;

namespace Transfer.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        // Manually register AutoMapper
        services.AddSingleton(_ => new MapperConfiguration(cfg =>
        {
            // Inventory
            cfg.AddProfile(new ProductProfile());
            cfg.AddProfile(new CategoryProfile());
            cfg.AddProfile(new ProductMovementProfile());
            cfg.AddProfile(new OrderProfile());
            cfg.AddProfile(new OrderDetailProfile());
            cfg.AddProfile(new SupplierProfile());
            cfg.AddProfile(new OrderTypeProfile());
            cfg.AddProfile(new OrderStatusProfile());
            cfg.AddProfile(new WarehouseProfile());

            //Sales
            cfg.AddProfile(new DistributionChannelProfile());
            cfg.AddProfile(new PriceItemProfile());

        }).CreateMapper());

        //services.AddValidatorsFromAssembly(assembly);

        //services.AddScoped<IValidator<SomeDto>, SomeDtoValidator>();
        //services.AddScoped<IValidator<AnotherDto>, AnotherDtoValidator>();

        return services;
    }
}