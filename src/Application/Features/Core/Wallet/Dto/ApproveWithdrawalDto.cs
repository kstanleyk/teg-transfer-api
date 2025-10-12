namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record ApproveWithdrawalDto(
    Guid LedgerId,
    string ApprovedBy = "System");