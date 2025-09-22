using System.Reflection;
using Agrovet.Application.Features.Core.AverageWeight;
using Agrovet.Application.Features.Inventory.Item;
using Agrovet.Application.Features.Inventory.ItemCategory;
using Agrovet.Application.Features.Inventory.ItemMovement;
using Agrovet.Application.Features.Inventory.Order;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Agrovet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        // Manually register AutoMapper
        services.AddSingleton(_ => new MapperConfiguration(cfg =>
        {
            //Core
            cfg.AddProfile(new AverageWeightProfile());

            // Inventory
            cfg.AddProfile(new ItemProfile());
            cfg.AddProfile(new ItemCategoryProfile());
            cfg.AddProfile(new ItemMovementProfile());
            cfg.AddProfile(new OrderProfile());
        }).CreateMapper());

        //services.AddValidatorsFromAssembly(assembly);

        //services.AddScoped<IValidator<SomeDto>, SomeDtoValidator>();
        //services.AddScoped<IValidator<AnotherDto>, AnotherDtoValidator>();

        return services;
    }
}