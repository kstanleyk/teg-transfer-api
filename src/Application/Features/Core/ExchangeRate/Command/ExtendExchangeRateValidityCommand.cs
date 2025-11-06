using MediatR;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Extend Exchange Rate Validity
public record ExtendExchangeRateValidityCommand(
    Guid ExchangeRateId,
    DateTime NewEffectiveTo,
    string UpdatedBy,
    string Reason = "Validity extended") : IRequest<Result>;