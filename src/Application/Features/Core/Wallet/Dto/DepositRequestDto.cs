namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record DepositRequestDto(
    decimal Amount,
    string CurrencyCode,
    string? Reference = null,
    string? Description = null);