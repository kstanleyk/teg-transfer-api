using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TegWallet.Application.Features.Core.Client;
using TegWallet.Application.Features.Core.Wallet;

namespace TegWallet.Application;

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
            // Core
            cfg.AddProfile(new ClientProfile());
            cfg.AddProfile(new WalletProfile());
        }).CreateMapper());

        //services.AddValidatorsFromAssembly(assembly);

        //services.AddScoped<IValidator<SomeDto>, SomeDtoValidator>();
        //services.AddScoped<IValidator<AnotherDto>, AnotherDtoValidator>();

        return services;
    }
}