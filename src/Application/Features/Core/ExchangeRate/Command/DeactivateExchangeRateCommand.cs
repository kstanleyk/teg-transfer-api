using MediatR;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.ExchangeRate.Command;

// Deactivate Exchange Rate
public record DeactivateExchangeRateCommand(
    Guid ExchangeRateId,
    string DeactivatedBy,
    string Reason = "Rate deactivated") : IRequest<Result>;