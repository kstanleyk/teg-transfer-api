namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class PurchaseReservationStatsDto
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int CompletedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalServiceFeeAmount { get; set; }
    public decimal AveragePurchaseAmount { get; set; }
    public Dictionary<string, int> ReservationsByPaymentMethod { get; set; } = new();
    public Dictionary<string, int> ReservationsByStatus { get; set; } = new();
    public List<MonthlyReservationStats> MonthlyStats { get; set; } = new();
}

public class MonthlyReservationStats
{
    public string Month { get; set; } = null!;
    public int ReservationCount { get; set; }
    public decimal TotalAmount { get; set; }
}