namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class WalletBalanceDto
{
    public Guid WalletId { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
    public BalanceStatus Status { get; set; }
    public List<BalanceBreakdownDto> Breakdown { get; set; } = [];
}

public class BalanceBreakdownDto
{
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
}

public enum BalanceStatus
{
    Healthy = 1,
    LowBalance = 2,
    Insufficient = 3,
    NoActivity = 4
}