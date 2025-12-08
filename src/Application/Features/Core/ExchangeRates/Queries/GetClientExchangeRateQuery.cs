using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetClientExchangeRateQuery(
    Guid ClientId,
    string BaseCurrencyCode,
    string TargetCurrencyCode,
    decimal Amount = 0,  // Amount in target currency, default to 0 for hierarchical rate
    DateTime? AsOfDate = null) : IRequest<Result<ClientWithExchangeRateDto?>>;

public class GetClientExchangeRateQueryHandler(
    IExchangeRateRepository exchangeRateRepository,
    IClientRepository clientRepository)
    : ClientWithExchangeRateHandlerBase, IRequestHandler<GetClientExchangeRateQuery, Result<ClientWithExchangeRateDto?>>
{
    private readonly IExchangeRateRepository _exchangeRateRepository = exchangeRateRepository;
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<Result<ClientWithExchangeRateDto?>> Handle(GetClientExchangeRateQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _clientRepository.GetClientForExchangeRateQueryAsync(query.ClientId);
            if (client == null)
                return Result<ClientWithExchangeRateDto?>.Failed("Client not found");

            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;

            // Convert string currency codes to Currency objects
            var baseCurrency = Currency.FromCode(query.BaseCurrencyCode);
            var targetCurrency = Currency.FromCode(query.TargetCurrencyCode);

            // If amount is 0, return hierarchical rate (backward compatibility)
            if (query.Amount == 0)
            {
                var rate = await _exchangeRateRepository.GetEffectiveRateForClientAsync(query.ClientId,
                    client.ClientGroupId, baseCurrency, targetCurrency, asOfDate);

                if (rate == null)
                    return Result<ClientWithExchangeRateDto?>.Succeeded(null);

                var exchangeRateDto = MapDto(client, rate);
                return Result<ClientWithExchangeRateDto?>.Succeeded(exchangeRateDto);
            }

            // For non-zero amounts, use tiered rate logic
            var applicationResult = await _exchangeRateRepository.GetApplicableRateWithTiersAsync(query.ClientId,
                client.ClientGroupId, baseCurrency, targetCurrency, query.Amount, asOfDate);

            var exchangeRate = applicationResult.ExchangeRate;

            if (exchangeRate == null)
                return Result<ClientWithExchangeRateDto?>.Succeeded(null);

            var appliedTier = applicationResult.AppliedTier;
            if (appliedTier != null) exchangeRate.ApplyTier(appliedTier);

            var rateDto = MapDto(client, exchangeRate);

            return Result<ClientWithExchangeRateDto?>.Succeeded(rateDto);
        }
        catch (Exception ex)
        {
            return Result<ClientWithExchangeRateDto?>.Failed($"An error occurred while retrieving exchange rate: {ex.Message}");
        }
    }
}