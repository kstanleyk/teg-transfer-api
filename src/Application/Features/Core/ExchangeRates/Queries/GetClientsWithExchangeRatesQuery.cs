using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetClientsWithExchangeRatesQuery(
    string BaseCurrency,
    string TargetCurrency)
    : IRequest<Result<IReadOnlyList<ClientWithExchangeRateDto>>>;

// GetClientsWithExchangeRatesQueryHandler.cs
public class GetClientsWithExchangeRatesQueryHandler(
    IClientRepository clientRepository,
    IExchangeRateRepository exchangeRateRepository)
    : ClientWithExchangeRateHandlerBase, IRequestHandler<GetClientsWithExchangeRatesQuery, Result<IReadOnlyList<ClientWithExchangeRateDto>>>
{
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;

    public async Task<Result<IReadOnlyList<ClientWithExchangeRateDto>>> Handle(
        GetClientsWithExchangeRatesQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate currency parameters
            var (baseCurrency, targetCurrency, validationResult) = ValidateAndParseCurrencies(query);
            if (!validationResult.Success)
                return Result<IReadOnlyList<ClientWithExchangeRateDto>>.Failed(validationResult.Message);

            // Get all active clients
            var clients = await _clientRepository.GetClientsForExchangeRateQueryAsync();

            if (!clients.Any())
                return Result<IReadOnlyList<ClientWithExchangeRateDto>>.Succeeded([]);

            // Get exchange rates for all clients
            var exchangeRatesByClientId = await _exchangeRateRepository.GetApplicableExchangeRatesForClientsAsync(
                clients, baseCurrency!, targetCurrency!);

            // Map to DTOs
            var clientDtos = MapToDtos(clients, exchangeRatesByClientId);

            return Result<IReadOnlyList<ClientWithExchangeRateDto>>.Succeeded(clientDtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ClientWithExchangeRateDto>>.Failed(
                $"An error occurred while retrieving clients with exchange rates: {ex.Message}");
        }
    }

    private static (Currency? baseCurrency, Currency? targetCurrency, Result validationResult)
        ValidateAndParseCurrencies(GetClientsWithExchangeRatesQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.BaseCurrency))
            return (null, null, Result.Failed("Base currency is required"));

        if (string.IsNullOrWhiteSpace(query.TargetCurrency))
            return (null, null, Result.Failed("Target currency is required"));

        if(!Currency.TryFromCode(query.BaseCurrency, out var baseCurrency))
            return (null, null, Result.Failed($"Invalid base currency: {query.BaseCurrency}"));

        //var baseCurrency = query.BaseCurrency.ParseCurrency();
        //if (!baseCurrency.HasValue)
        //    return (null, null, Result.Failed($"Invalid base currency: {query.BaseCurrency}"));

        if (!Currency.TryFromCode(query.TargetCurrency, out var targetCurrency))
            return (null, null, Result.Failed($"Invalid base currency: {query.BaseCurrency}"));

        //var targetCurrency = query.TargetCurrency.ParseCurrency();
        //if (!targetCurrency.HasValue)
        //    return (null, null, Result.Failed($"Invalid target currency: {query.TargetCurrency}"));

        if (baseCurrency == targetCurrency)
            return (null, null, Result.Failed("Base currency and target currency cannot be the same"));

        return (baseCurrency, targetCurrency, Result.Succeeded());
    }

    private IReadOnlyList<ClientWithExchangeRateDto> MapToDtos(
        IReadOnlyList<Client> clients,
        Dictionary<Guid, ExchangeRate?> exchangeRatesByClientId)
    {
        var result = new List<ClientWithExchangeRateDto>();

        foreach (var client in clients)
        {
            exchangeRatesByClientId.TryGetValue(client.Id, out var exchangeRate);

            var dto = MapDto(client, exchangeRate);

            result.Add(dto);
        }

        return result;
    }

}