namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record RejectWithdrawalDto(
    Guid LedgerId,
    string Reason,
    string RejectedBy = "System");