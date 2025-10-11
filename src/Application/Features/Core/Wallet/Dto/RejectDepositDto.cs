namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record RejectDepositDto(
    Guid LedgerId,
    string Reason,
    string RejectedBy = "System");