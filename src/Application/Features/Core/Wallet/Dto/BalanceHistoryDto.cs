namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class BalanceHistoryDto
{
    public Guid WalletId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public List<BalanceSnapshotDto> Snapshots { get; set; } = new();
}