namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class CheckBalanceRequest
{
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = null!;
}