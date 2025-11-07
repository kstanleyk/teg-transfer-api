using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.ExchangeRates.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateRepository(IExchangeRateHistoryRepository echExchangeRateHistoryRepository, IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRate, Guid>(databaseFactory), IExchangeRateRepository
{
    public async Task<ExchangeRate?> GetApplicableRateAsync(
        Guid? clientId,
        Guid? clientGroupId,
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime asOfDate)
    {
        try
        {
            // Get all active rates for the currency pair that are effective at the given date
            var rates = await DbSet
                .Include(er => er.ClientGroup)
                .Include(er => er.Client)
                .Where(er => er.BaseCurrency == baseCurrency &&
                            er.TargetCurrency == targetCurrency &&
                            er.IsActive &&
                            er.EffectiveFrom <= asOfDate &&
                            (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                .OrderByDescending(er => er.Type) // Individual (3) > Group (2) > General (1)
                .ThenByDescending(er => er.EffectiveFrom) // Most recent first
                .ToListAsync();

            if (!rates.Any())
            {
                //_logger.LogWarning("No exchange rates found for {BaseCurrency} to {TargetCurrency} as of {AsOfDate}",
                    //baseCurrency.Code, targetCurrency.Code, asOfDate);
                return null;
            }

            // Priority order: Individual > Group > General
            ExchangeRate? applicableRate = null;

            // 1. Check for individual rate for the specific client
            if (clientId.HasValue)
            {
                applicableRate = rates.FirstOrDefault(er =>
                    er.Type == RateType.Individual && er.ClientId == clientId.Value);

                if (applicableRate != null)
                {
                    //_logger.LogDebug("Found individual rate for client {ClientId}: {Rate}",
                    //    clientId, applicableRate.EffectiveRate);
                    return applicableRate;
                }
            }

            // 2. Check for group rate for the client's group
            if (clientGroupId.HasValue)
            {
                applicableRate = rates.FirstOrDefault(er =>
                    er.Type == RateType.Group && er.ClientGroupId == clientGroupId.Value);

                if (applicableRate != null)
                {
                    //_logger.LogDebug("Found group rate for group {GroupId}: {Rate}",
                    //    clientGroupId, applicableRate.EffectiveRate);
                    return applicableRate;
                }
            }

            // 3. Use general rate (applies to all clients)
            applicableRate = rates.FirstOrDefault(er => er.Type == RateType.General);

            if (applicableRate != null)
            {
                //_logger.LogDebug("Found general rate: {Rate}", applicableRate.EffectiveRate);
                return applicableRate;
            }

            //_logger.LogWarning("No applicable exchange rate found for client {ClientId}, group {GroupId}, {BaseCurrency} to {TargetCurrency}",
                //clientId, clientGroupId, baseCurrency.Code, targetCurrency.Code);

            return null;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error getting applicable exchange rate for client {ClientId}, group {GroupId}, {BaseCurrency} to {TargetCurrency}",
            //    clientId, clientGroupId, baseCurrency.Code, targetCurrency.Code);
            throw;
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> CreateGeneralExchangeRateAsync(
        CreateGeneralExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Use the domain factory method to create the exchange rate
            var exchangeRate = ExchangeRate.CreateGeneralRate(
                parameters.BaseCurrency,
                parameters.TargetCurrency,
                parameters.BaseCurrencyValue,
                parameters.TargetCurrencyValue,
                parameters.Margin,
                parameters.EffectiveFrom,
                parameters.CreatedBy,
                parameters.Source,
                parameters.EffectiveTo);

            // Add the exchange rate to the context
            DbSet.Add(exchangeRate);

            // Create history record for the creation
            var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> CreateGroupExchangeRateAsync(
    CreateGroupExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Use the domain factory method to create the exchange rate
            var exchangeRate = ExchangeRate.CreateGroupRate(
                parameters.BaseCurrency,
                parameters.TargetCurrency,
                parameters.BaseCurrencyValue,
                parameters.TargetCurrencyValue,
                parameters.Margin,
                parameters.ClientGroupId,
                parameters.EffectiveFrom,
                parameters.CreatedBy,
                parameters.Source,
                parameters.EffectiveTo);

            // Add the exchange rate to the context
            DbSet.Add(exchangeRate);

            // Create history record for the creation
            var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> DeactivateExchangeRateAsync(
    DeactivateExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var exchangeRate = await DbSet
                .Include(er => er.Client)
                .Include(er => er.ClientGroup)
                .FirstOrDefaultAsync(er => er.Id == parameters.ExchangeRateId);

            if (exchangeRate == null)
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NotFound);

            if (!exchangeRate.IsActive)
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.NothingModified);

            // Deactivate the exchange rate
            exchangeRate.Deactivate();

            // Create history record for deactivation
            var history = ExchangeRateHistory.CreateFromExchangeRate(
                exchangeRate,
                parameters.Reason,
                parameters.DeactivatedBy,
                "DEACTIVATED");

            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> ExtendExchangeRateValidityAsync(
    ExtendExchangeRateValidityParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var exchangeRate = await DbSet
                .Include(er => er.Client)
                .Include(er => er.ClientGroup)
                .FirstOrDefaultAsync(er => er.Id == parameters.ExchangeRateId);

            if (exchangeRate == null)
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NotFound);

            if (!exchangeRate.IsActive)
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Error);

            // Extend the validity
            exchangeRate.ExtendValidity(parameters.NewEffectiveTo);

            // Create history record for extension
            var history = ExchangeRateHistory.CreateFromExchangeRate(
                exchangeRate,
                parameters.Reason,
                parameters.UpdatedBy,
                "EXTENDED");

            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> CreateIndividualExchangeRateAsync(
    CreateIndividualExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Use the domain factory method to create the exchange rate
            var exchangeRate = ExchangeRate.CreateIndividualRate(
                parameters.BaseCurrency,
                parameters.TargetCurrency,
                parameters.BaseCurrencyValue,
                parameters.TargetCurrencyValue,
                parameters.Margin,
                parameters.ClientId,
                parameters.EffectiveFrom,
                parameters.CreatedBy,
                parameters.Source,
                parameters.EffectiveTo);

            // Add the exchange rate to the context
            DbSet.Add(exchangeRate);

            // Create history record for the creation
            var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ExchangeRate>> UpdateExchangeRateAsync(
    UpdateExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var exchangeRate = await DbSet
                .Include(er => er.Client)
                .Include(er => er.ClientGroup)
                .FirstOrDefaultAsync(er => er.Id == parameters.ExchangeRateId);

            if (exchangeRate == null)
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NotFound);

            // Store previous values for history
            var previousBaseValue = exchangeRate.BaseCurrencyValue;
            var previousTargetValue = exchangeRate.TargetCurrencyValue;
            var previousMargin = exchangeRate.Margin;

            // Update the exchange rate values
            exchangeRate.UpdateCurrencyValues(
                parameters.NewBaseCurrencyValue,
                parameters.NewTargetCurrencyValue,
                parameters.NewMargin);

            // Create history record for the update
            var history = ExchangeRateHistory.CreateForUpdate(
                exchangeRate,
                previousBaseValue,
                previousTargetValue,
                previousMargin,
                parameters.UpdatedBy,
                parameters.Reason);

            await echExchangeRateHistoryRepository.AddAsync(history);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetActiveRatesAsync(Currency baseCurrency, Currency targetCurrency,
        DateTime effectiveDate)
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

    public async Task<IReadOnlyList<ExchangeRate>> GetHistoricalRatesAsync(Currency baseCurrency,
        Currency targetCurrency, DateTime from, DateTime to)
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

    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(er => er.Client)
            .Include(er => er.ClientGroup)
            .FirstOrDefaultAsync(er => er.Id == id);
    }

    public async Task<ExchangeRate?> GetEffectiveRateForClientAsync(
            Guid clientId,
            Guid? clientGroupId,
            Currency baseCurrency,
            Currency targetCurrency,
            DateTime asOfDate,
            CancellationToken cancellationToken = default)
    {
        // Try individual rate first
        var individualRate = await DbSet
            .Include(er => er.Client)
            .Include(er => er.ClientGroup)
            .Where(er => er.ClientId == clientId &&
                        er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .OrderByDescending(er => er.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (individualRate != null)
            return individualRate;

        // Try group rate
        if (clientGroupId.HasValue)
        {
            var groupRate = await DbSet
                .Include(er => er.ClientGroup)
                .Where(er => er.ClientGroupId == clientGroupId &&
                            er.BaseCurrency == baseCurrency &&
                            er.TargetCurrency == targetCurrency &&
                            er.IsActive &&
                            er.EffectiveFrom <= asOfDate &&
                            (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                .OrderByDescending(er => er.EffectiveFrom)
                .FirstOrDefaultAsync(cancellationToken);

            if (groupRate != null)
                return groupRate;
        }

        // Fall back to general rate
        var generalRate = await DbSet
            .Include(er => er.ClientGroup)
            .Where(er => er.Type == RateType.General &&
                        er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .OrderByDescending(er => er.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        return generalRate;
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetClientAvailableRatesAsync(
        Guid clientId,
        Currency baseCurrency,
        DateTime asOfDate)
    {
        // Get individual rates for this client
        var individualRates = await DbSet
            .Include(er => er.Client)
            .Include(er => er.ClientGroup)
            .Where(er => er.ClientId == clientId &&
                        er.BaseCurrency == baseCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .ToListAsync();

        // For group and general rates, we'll get all and let the handler or service determine hierarchy
        var groupAndGeneralRates = await DbSet
            .Include(er => er.ClientGroup)
            .Where(er => (er.Type == RateType.Group || er.Type == RateType.General) &&
                        er.BaseCurrency == baseCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .ToListAsync();

        // Combine all rates
        var allRates = individualRates.Concat(groupAndGeneralRates).ToList();

        return allRates;
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetExchangeRateHistoryAsync(Currency baseCurrency,
        Currency targetCurrency, DateTime fromDate, DateTime toDate, RateType? type, Guid? clientOrGroupId)
    {
        var query = DbSet
            .Where(er => er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.EffectiveFrom >= fromDate &&
                        er.EffectiveFrom <= toDate);

        if (type.HasValue)
        {
            query = query.Where(er => er.Type == type.Value);
        }

        if (clientOrGroupId.HasValue)
        {
            query = query.Where(er => er.ClientId == clientOrGroupId || er.ClientGroupId == clientOrGroupId);
        }

        return await query
            .OrderBy(er => er.EffectiveFrom)
            .ToListAsync();
    }

}