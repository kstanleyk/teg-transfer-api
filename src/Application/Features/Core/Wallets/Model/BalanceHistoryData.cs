namespace TegWallet.Application.Features.Core.Wallets.Model;

public class BalanceHistoryData
{
    public List<Domain.Entity.Core.Ledger> Transactions { get; set; } = [];
    public decimal StartingBalance { get; set; }
    public List<DailyBalance> DailyBalances { get; set; } = [];
}