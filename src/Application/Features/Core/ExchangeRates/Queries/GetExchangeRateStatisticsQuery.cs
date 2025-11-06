using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetExchangeRateStatisticsQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime FromDate,
    DateTime ToDate,
    RateType? Type = null,
    Guid? ClientOrGroupId = null) : IRequest<ExchangeRateStatisticsDto>;