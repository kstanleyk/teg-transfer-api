using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetExchangeRateByTypeQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    RateType Type,
    Guid? ClientOrGroupId = null,
    DateTime? AsOfDate = null) : IRequest<ExchangeRateDto?>;