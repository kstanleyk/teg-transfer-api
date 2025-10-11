namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class BalanceSnapshotDto
{
    public DateTime Timestamp { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public int TransactionCount { get; set; }
    public decimal NetChange { get; set; }
    public string PeriodLabel { get; set; } = null!;
}