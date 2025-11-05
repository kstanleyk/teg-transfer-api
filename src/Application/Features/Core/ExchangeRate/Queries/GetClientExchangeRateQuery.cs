using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetClientExchangeRateQuery(
    Guid ClientId,
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime? AsOfDate = null) : IRequest<ExchangeRateDto?>;

public class GetClientExchangeRateQueryHandler(
    IExchangeRateRepository exchangeRateRepository,
    IClientRepository clientRepository)
    : IRequestHandler<GetClientExchangeRateQuery, ExchangeRateDto?>
{
    public async Task<ExchangeRateDto?> Handle(GetClientExchangeRateQuery query, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client == null)
            return null;

        var asOfDate = query.AsOfDate ?? DateTime.UtcNow;

        // Get the most specific rate available (Individual > Group > General)
        var rate = await exchangeRateRepository.GetEffectiveRateForClientAsync(
            query.ClientId,
            client.ClientGroupId,
            query.BaseCurrency,
            query.TargetCurrency,
            asOfDate,
            cancellationToken);

        return rate?.ToDto();
    }
}