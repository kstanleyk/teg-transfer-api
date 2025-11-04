namespace TegWallet.Application.Features.Core.Wallets.Dto;

public record WithdrawalRequestDto(
    decimal Amount,
    string CurrencyCode,
    string? Description = null);