namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class BalanceSnapshotDto
{
    public DateTime Date { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
}