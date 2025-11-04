namespace TegWallet.Application.Features.Core.Wallets.Dto;

public record ApproveDepositDto(
    Guid LedgerId,
    string ApprovedBy = "System");