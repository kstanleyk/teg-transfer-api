using TegWallet.Application.Features.Core.Wallet.Query;

namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class BalanceHistoryDto
{
    public Guid WalletId { get; set; }
    public Guid ClientId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public BalanceHistoryPeriod Period { get; set; }
    public List<BalanceSnapshotDto> Snapshots { get; set; } = new();
    public BalanceSummaryDto Summary { get; set; } = null!;
}