using MediatR;
using TegWallet.Application.Features.Core.Currencies.Dto;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.Currencies.Query;

// Query to return all valid currencies
public record CurrenciesQuery : IRequest<Result<CurrencyDto[]>>;

public class CurrenciesQueryHandler : IRequestHandler<CurrenciesQuery, Result<CurrencyDto[]>>
{
    public Task<Result<CurrencyDto[]>> Handle(CurrenciesQuery query, CancellationToken cancellationToken)
    {
        var currencies = Domain.ValueObjects.Currency.All
            .Select(c => new CurrencyDto
            {
                Code = c.Code,
                Symbol = c.Symbol,
                DecimalPlaces = c.DecimalPlaces
            })
            .ToArray();

        return Task.FromResult(Result<CurrencyDto[]>.Succeeded(currencies, "Valid currencies retrieved successfully."));
    }
}