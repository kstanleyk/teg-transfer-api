namespace TegWallet.Application.Features.Core.Wallets.Model;

public class DailyBalance
{
    public DateTime Date { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int TransactionCount { get; set; }
}