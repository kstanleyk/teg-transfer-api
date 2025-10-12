namespace TegWallet.Application.Features.Core.Wallet.Dto;

public record ApproveDepositDto(
    Guid LedgerId,
    string ApprovedBy = "System");