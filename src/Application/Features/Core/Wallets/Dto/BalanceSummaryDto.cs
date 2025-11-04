namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class BalanceSummaryDto
{
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal NetChange { get; set; }
    public decimal PercentageChange { get; set; }
    public decimal HighestBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public decimal AverageBalance { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalFees { get; set; }
}