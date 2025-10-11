namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record WithdrawalRequestDto(
    decimal Amount,
    string CurrencyCode,
    string? Description = null);