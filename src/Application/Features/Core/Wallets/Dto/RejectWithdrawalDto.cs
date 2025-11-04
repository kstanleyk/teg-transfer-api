namespace TegWallet.Application.Features.Core.Wallets.Dto;

public record RejectWithdrawalDto(
    Guid LedgerId,
    string Reason,
    string RejectedBy = "System");