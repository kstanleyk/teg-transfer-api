using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetExchangeRateStatisticsQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime FromDate,
    DateTime ToDate,
    RateType? Type = null,
    Guid? ClientOrGroupId = null) : IRequest<ExchangeRateStatisticsDto>;