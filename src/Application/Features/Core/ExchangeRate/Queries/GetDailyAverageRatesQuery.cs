using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetDailyAverageRatesQuery(
    Currency BaseCurrency,
    Currency TargetCurrency,
    DateTime FromDate,
    DateTime ToDate,
    RateType? Type = null) : IRequest<IReadOnlyList<DailyRateDto>>;