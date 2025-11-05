using MediatR;
using TegWallet.Application.Features.Core.ExchangeRate.Dtos;

namespace TegWallet.Application.Features.Core.ExchangeRate.Queries;

public record GetMarginAnalysisQuery(
    DateTime FromDate,
    DateTime ToDate,
    Guid? ClientGroupId = null) : IRequest<IReadOnlyList<MarginAnalysisDto>>;