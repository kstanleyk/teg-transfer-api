using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRate, Guid>(databaseFactory), IExchangeRateRepository
{
    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetActiveRatesAsync(Currency baseCurrency, Currency targetCurrency, DateTime effectiveDate)
    {
        return await DbSet
            .Where(x => x.BaseCurrency == baseCurrency &&
                       x.TargetCurrency == targetCurrency &&
                       x.IsActive &&
                       x.EffectiveFrom <= effectiveDate &&
                       (x.EffectiveTo == null || x.EffectiveTo >= effectiveDate))
            .OrderByDescending(x => x.Type) // Individual first, then Group, then General
            .ThenByDescending(x => x.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetRatesByClientAsync(Guid clientId)
    {
        return await DbSet
            .Where(x => x.ClientId == clientId && x.IsActive)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetHistoricalRatesAsync(Currency baseCurrency, Currency targetCurrency, DateTime from, DateTime to)
    {
        return await DbSet
            .Where(x => x.BaseCurrency == baseCurrency &&
                       x.TargetCurrency == targetCurrency &&
                       ((x.EffectiveFrom >= from && x.EffectiveFrom <= to) ||
                        (x.EffectiveTo >= from && x.EffectiveTo <= to) ||
                        (x.EffectiveFrom <= from && (x.EffectiveTo == null || x.EffectiveTo >= to))))
            .OrderBy(x => x.EffectiveFrom)
            .ToListAsync();
    }
}