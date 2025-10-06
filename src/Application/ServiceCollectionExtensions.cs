using Agrovet.Application.Features.Inventory.Order;
using Agrovet.Application.Features.Inventory.OrderDetail;
using Agrovet.Application.Features.Inventory.OrderStatus;
using Agrovet.Application.Features.Inventory.OrderType;
using Agrovet.Application.Features.Inventory.Supplier;
using Agrovet.Application.Features.Sales.DistributionChannel;
using Agrovet.Application.Features.Sales.PriceItem;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Agrovet.Application.Features.Inventory.Product;
using Agrovet.Application.Features.Inventory.ProductCategory;
using Agrovet.Application.Features.Inventory.ProductMovement;

namespace Agrovet.Application;

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
            cfg.AddProfile(new ProductCategoryProfile());
            cfg.AddProfile(new ProductMovementProfile());
            cfg.AddProfile(new OrderProfile());
            cfg.AddProfile(new OrderDetailProfile());
            cfg.AddProfile(new SupplierProfile());
            cfg.AddProfile(new OrderTypeProfile());
            cfg.AddProfile(new OrderStatusProfile());

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