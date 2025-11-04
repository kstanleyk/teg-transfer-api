namespace TegWallet.Application.Features.Core.Wallets.Dto;

public record RejectDepositDto(
    Guid LedgerId,
    string Reason,
    string RejectedBy = "System");