using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateHistoryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRateHistory, Guid>(databaseFactory), IExchangeRateHistoryRepository
{
    public async Task<IReadOnlyList<ExchangeRateHistory>> GetExchangeRateChangeHistoryAsync(Guid exchangeRateId)
    {
        return await DbSet
            .Where(erh => erh.ExchangeRateId == exchangeRateId)
            .OrderByDescending(erh => erh.ChangedAt)
            .ToListAsync();
    }
}