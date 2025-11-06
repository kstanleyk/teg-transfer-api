using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TegWallet.Application.Features.Core.ClientGroups;
using TegWallet.Application.Features.Core.Clients;
using TegWallet.Application.Features.Core.ExchangeRates;
using TegWallet.Application.Features.Core.Ledgers;
using TegWallet.Application.Features.Core.Purchases;
using TegWallet.Application.Features.Core.Wallets;

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
            cfg.AddProfile(new PurchaseProfile());
            cfg.AddProfile(new LedgerProfile());
            cfg.AddProfile(new ExchangeRateProfile());
            cfg.AddProfile(new ClientGroupProfile());
        }).CreateMapper());

        //services.AddValidatorsFromAssembly(assembly);

        //services.AddScoped<IValidator<SomeDto>, SomeDtoValidator>();
        //services.AddScoped<IValidator<AnotherDto>, AnotherDtoValidator>();

        return services;
    }
}