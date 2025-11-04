namespace TegWallet.Application.Features.Core.Wallets.Dto;

public record ApproveWithdrawalDto(
    Guid LedgerId,
    string ApprovedBy = "System");