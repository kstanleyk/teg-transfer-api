using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Features.Core.Wallet.Model;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Application.Features.Core.Wallet.Query;

// Query to get balance history for a period
public record GetBalanceHistoryQuery(Guid ClientId, DateTime FromDate, DateTime ToDate, BalanceHistoryPeriod Period = BalanceHistoryPeriod.Daily)
    : IRequest<BalanceHistoryDto>;

public class GetBalanceHistoryQueryHandler(
        IWalletRepository walletRepository)
        : IRequestHandler<GetBalanceHistoryQuery, BalanceHistoryDto>
{
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<BalanceHistoryDto> Handle(GetBalanceHistoryQuery query, CancellationToken cancellationToken)
    {
        // Validate date range
        if (query.FromDate > query.ToDate)
            throw new InvalidOperationException("From date cannot be after to date");

        if ((query.ToDate - query.FromDate).TotalDays > 365)
            throw new InvalidOperationException("Date range cannot exceed 1 year");

        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);
        if (wallet == null)
            throw new InvalidOperationException($"Wallet not found for client ID: {query.ClientId}");

        var historyData = await _walletRepository.GetBalanceHistoryDataAsync(wallet.Id, query.FromDate, query.ToDate);
        var balanceHistory = await BuildBalanceHistoryDto(wallet, historyData, query.FromDate, query.ToDate, query.Period);

        return balanceHistory;
    }

    private async Task<BalanceHistoryDto> BuildBalanceHistoryDto(Domain.Entity.Core.Wallet wallet, BalanceHistoryData historyData,
        DateTime fromDate, DateTime toDate, BalanceHistoryPeriod period)
    {
        var snapshots = await BuildBalanceSnapshots(historyData, period);
        var summary = BuildBalanceSummary(historyData, snapshots);

        return new BalanceHistoryDto
        {
            WalletId = wallet.Id,
            ClientId = wallet.ClientId,
            CurrencyCode = wallet.BaseCurrency.Code,
            FromDate = fromDate,
            ToDate = toDate,
            Period = period,
            Snapshots = snapshots,
            Summary = summary
        };
    }

    private async Task<List<BalanceSnapshotDto>> BuildBalanceSnapshots(BalanceHistoryData historyData, BalanceHistoryPeriod period)
    {
        var snapshots = new List<BalanceSnapshotDto>();

        switch (period)
        {
            case BalanceHistoryPeriod.Daily:
                snapshots = historyData.DailyBalances.Select(db => new BalanceSnapshotDto
                {
                    Timestamp = db.Date,
                    TotalBalance = db.TotalBalance,
                    AvailableBalance = db.AvailableBalance,
                    PendingBalance = 0, // Would need additional logic to calculate pending
                    TransactionCount = db.TransactionCount,
                    NetChange = CalculateNetChange(historyData.DailyBalances, db.Date),
                    PeriodLabel = db.Date.ToString("MMM dd")
                }).ToList();
                break;

            case BalanceHistoryPeriod.Weekly:
                snapshots = await BuildWeeklySnapshots(historyData);
                break;

            case BalanceHistoryPeriod.Monthly:
                snapshots = await BuildMonthlySnapshots(historyData);
                break;

            case BalanceHistoryPeriod.Hourly:
                snapshots = await BuildHourlySnapshots(historyData);
                break;
        }

        return snapshots;
    }

    private static Task<List<BalanceSnapshotDto>> BuildWeeklySnapshots(BalanceHistoryData historyData)
    {
        var weeklyGroups = historyData.DailyBalances
            .GroupBy(db => GetWeekKey(db.Date))
            .OrderBy(g => g.Key)
            .ToList();

        var snapshots = new List<BalanceSnapshotDto>();
        var previousWeekBalance = historyData.StartingBalance;

        foreach (var weekGroup in weeklyGroups)
        {
            var lastDayOfWeek = weekGroup.OrderByDescending(x => x.Date).First();
            var weekTransactions = historyData.Transactions
                .Where(t => GetWeekKey(t.Timestamp) == weekGroup.Key)
                .ToList();

            snapshots.Add(new BalanceSnapshotDto
            {
                Timestamp = lastDayOfWeek.Date,
                TotalBalance = lastDayOfWeek.TotalBalance,
                AvailableBalance = lastDayOfWeek.AvailableBalance,
                TransactionCount = weekTransactions.Count,
                NetChange = lastDayOfWeek.TotalBalance - previousWeekBalance,
                PeriodLabel = $"Week {GetWeekOfYear(lastDayOfWeek.Date)}"
            });

            previousWeekBalance = lastDayOfWeek.TotalBalance;
        }

        return Task.FromResult(snapshots);
    }

    private static Task<List<BalanceSnapshotDto>> BuildMonthlySnapshots(BalanceHistoryData historyData)
    {
        var monthlyGroups = historyData.DailyBalances
            .GroupBy(db => new { db.Date.Year, db.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToList();

        var snapshots = new List<BalanceSnapshotDto>();
        var previousMonthBalance = historyData.StartingBalance;

        foreach (var monthGroup in monthlyGroups)
        {
            var lastDayOfMonth = monthGroup.OrderByDescending(x => x.Date).First();
            var monthTransactions = historyData.Transactions
                .Where(t => t.Timestamp.Year == monthGroup.Key.Year && t.Timestamp.Month == monthGroup.Key.Month)
                .ToList();

            snapshots.Add(new BalanceSnapshotDto
            {
                Timestamp = lastDayOfMonth.Date,
                TotalBalance = lastDayOfMonth.TotalBalance,
                AvailableBalance = lastDayOfMonth.AvailableBalance,
                TransactionCount = monthTransactions.Count,
                NetChange = lastDayOfMonth.TotalBalance - previousMonthBalance,
                PeriodLabel = lastDayOfMonth.Date.ToString("MMM yyyy")
            });

            previousMonthBalance = lastDayOfMonth.TotalBalance;
        }

        return Task.FromResult(snapshots);
    }

    private static Task<List<BalanceSnapshotDto>> BuildHourlySnapshots(BalanceHistoryData historyData)
    {
        // For hourly, we'll sample every 4 hours for demonstration
        // In a real implementation, you'd want more sophisticated sampling
        var hourlySnapshots = new List<BalanceSnapshotDto>();
        var currentBalance = historyData.StartingBalance;

        // This is a simplified implementation - in reality, you'd need more complex logic
        // for hourly balance tracking
        foreach (var transaction in historyData.Transactions.OrderBy(t => t.Timestamp))
        {
            currentBalance += transaction.Type == TransactionType.Deposit ?
                transaction.Amount.Amount : -transaction.Amount.Amount;

            // Only add snapshot every 4 hours to avoid too many data points
            if (transaction.Timestamp.Hour % 4 == 0)
            {
                hourlySnapshots.Add(new BalanceSnapshotDto
                {
                    Timestamp = transaction.Timestamp,
                    TotalBalance = currentBalance,
                    AvailableBalance = currentBalance,
                    TransactionCount = 1,
                    NetChange = transaction.Type == TransactionType.Deposit ?
                        transaction.Amount.Amount : -transaction.Amount.Amount,
                    PeriodLabel = transaction.Timestamp.ToString("HH:mm")
                });
            }
        }

        return Task.FromResult(hourlySnapshots);
    }

    private BalanceSummaryDto BuildBalanceSummary(BalanceHistoryData historyData, List<BalanceSnapshotDto> snapshots)
    {
        if (!snapshots.Any())
            return new BalanceSummaryDto();

        var startingBalance = historyData.StartingBalance;
        var endingBalance = snapshots.Last().TotalBalance;
        var netChange = endingBalance - startingBalance;
        var percentageChange = startingBalance != 0 ? (netChange / startingBalance) * 100 : 0;

        var transactions = historyData.Transactions;
        var totalDeposits = transactions
            .Where(t => t.Type == TransactionType.Deposit && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Amount.Amount);

        var totalWithdrawals = transactions
            .Where(t => t.Type == TransactionType.Withdrawal && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Amount.Amount);

        var totalPurchases = transactions
            .Where(t => t.Type == TransactionType.Purchase && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Amount.Amount);

        var totalFees = transactions
            .Where(t => t.Type == TransactionType.ServiceFee && t.Status == TransactionStatus.Completed)
            .Sum(t => t.Amount.Amount);

        return new BalanceSummaryDto
        {
            StartingBalance = startingBalance,
            EndingBalance = endingBalance,
            NetChange = netChange,
            PercentageChange = percentageChange,
            HighestBalance = snapshots.Max(s => s.TotalBalance),
            LowestBalance = snapshots.Min(s => s.TotalBalance),
            AverageBalance = snapshots.Average(s => s.TotalBalance),
            TotalTransactions = transactions.Count,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals,
            TotalPurchases = totalPurchases,
            TotalFees = totalFees
        };
    }

    // Helper methods
    private static string GetWeekKey(DateTime date)
    {
        return $"{date.Year}-{GetWeekOfYear(date)}";
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
    }

    private static decimal CalculateNetChange(List<DailyBalance> dailyBalances, DateTime date)
    {
        var currentDay = dailyBalances.FirstOrDefault(db => db.Date == date);
        var previousDay = dailyBalances.LastOrDefault(db => db.Date < date);

        if (currentDay == null) return 0;
        if (previousDay == null) return currentDay.TotalBalance;

        return currentDay.TotalBalance - previousDay.TotalBalance;
    }
}

public enum BalanceHistoryPeriod
{
    Hourly = 1,
    Daily = 2,
    Weekly = 3,
    Monthly = 4
}