using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetRateEffectivenessReportQuery(
    DateTime FromDate,
    DateTime ToDate) : IRequest<RateEffectivenessReportDto>;