using MediatR;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public record GetMarginAnalysisQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? ClientGroupId = null) : IRequest<IReadOnlyList<MarginAnalysisDto>>;