namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class BalanceSufficiencyDto
{
    public Guid WalletId { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal RequiredAmount { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public bool IsSufficient { get; set; }
    public decimal Difference { get; set; }
    public string Message { get; set; } = null!;
}