namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class SimpleBalanceDto
{
    public Guid WalletId { get; set; }
    public decimal AvailableBalance { get; set; }
    public string CurrencyCode { get; set; } = null!;
}