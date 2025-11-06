using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetRateEffectivenessReportQuery(
    DateTime FromDate,
    DateTime ToDate) : IRequest<RateEffectivenessReportDto>;