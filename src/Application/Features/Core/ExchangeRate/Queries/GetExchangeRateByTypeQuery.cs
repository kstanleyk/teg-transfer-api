using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetExchangeRateByTypeQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    RateType Type,
    Guid? ClientOrGroupId = null,
    DateTime? AsOfDate = null) : IRequest<ExchangeRateDto?>;