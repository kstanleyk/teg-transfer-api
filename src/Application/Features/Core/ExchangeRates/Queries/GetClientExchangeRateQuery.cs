using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetClientExchangeRateQuery(
    Guid ClientId,
    Currency BaseCurrency,
    Currency TargetCurrency,
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

            // Get the most specific rate available (Individual > Group > General)
            var rate = await _exchangeRateRepository.GetEffectiveRateForClientAsync(
                query.ClientId,
                client.ClientGroupId,
                query.BaseCurrency,
                query.TargetCurrency,
                asOfDate);

            if (rate == null)
                return Result<ClientWithExchangeRateDto?>.Succeeded(null);

            var rateDto = MapDto(client, rate);
            return Result<ClientWithExchangeRateDto?>.Succeeded(rateDto);
        }
        catch (Exception ex)
        {
            // Log exception here if needed
            return Result<ClientWithExchangeRateDto?>.Failed($"An error occurred while retrieving exchange rate: {ex.Message}");
        }
    }
}