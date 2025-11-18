using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.ExchangeRates.Command;
using TegWallet.Application.Features.Core.ExchangeRates.Queries;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ExchangeRateRepository(IMinimumAmountConfigurationRepository minimumAmountConfigurationRepository,  IDatabaseFactory databaseFactory)
    : DataRepository<ExchangeRate, Guid>(databaseFactory), IExchangeRateRepository
{
    public async Task<ExchangeRateApplicationResult> GetApplicableRateWithTiersAsync(
        Guid? clientId,
        Guid? clientGroupId,
        Currency baseCurrency,
        Currency targetCurrency,
        decimal transactionAmount, // Target currency amount
        DateTime asOfDate)
    {
        try
        {
            // 1. Get the applicable minimum amount configuration for this currency pair
            var minAmountConfig = await minimumAmountConfigurationRepository.GetApplicableMinimumAmountAsync(
                baseCurrency, targetCurrency, asOfDate);

            var minimumAmount = minAmountConfig?.MinimumAmount ?? 0m;

            // 2. Check if transaction qualifies for hierarchical rates
            if (transactionAmount >= minimumAmount)
            {
                // Use existing hierarchical system (Individual → Group → General)
                var hierarchicalRate = await GetApplicableRateAsync(
                    clientId, clientGroupId, baseCurrency, targetCurrency, asOfDate);

                return new ExchangeRateApplicationResult
                {
                    ExchangeRate = hierarchicalRate,
                    AppliedTier = null,
                    RateType = hierarchicalRate?.Type ?? RateType.General,
                    IsTieredRate = false,
                    MinimumAmount = minimumAmount,
                    EffectiveRate = hierarchicalRate?.EffectiveRate ?? 0m,
                    EffectiveMargin = hierarchicalRate?.Margin ?? 0m
                };
            }
            else
            {
                // 3. Use tiered pricing system - ONLY for general rates
                return await GetTieredGeneralRateAsync(
                    baseCurrency, targetCurrency, transactionAmount, asOfDate, minimumAmount);
            }
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Error getting applicable rate with tiers for client {clientId}", ex);
        }
    }

    public async Task ManageExchangeRateTiersAsync(Guid exchangeRateId, List<ExchangeRateTierRequest> tierRequests)
    {
        var exchangeRate = await DbSet
            .Include(er => er.Tiers)
            .FirstOrDefaultAsync(er => er.Id == exchangeRateId);

        if (exchangeRate == null)
            throw new ArgumentException($"Exchange rate with ID {exchangeRateId} not found");

        // Clear existing tiers
        exchangeRate.ClearTiers();

        // Add new tiers
        foreach (var tierRequest in tierRequests)
        {
            exchangeRate.AddTier(
                tierRequest.MinAmount,
                tierRequest.MaxAmount,
                tierRequest.Rate,
                tierRequest.Margin,
                tierRequest.CreatedBy);
        }

        await SaveChangesAsync();
    }

    private async Task<ExchangeRateApplicationResult> GetTieredGeneralRateAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        decimal transactionAmount,
        DateTime asOfDate,
        decimal minimumAmount)
    {
        // Get ONLY the general rate with its tiers
        var generalRate = await GetGeneralRateWithTiersAsync(baseCurrency, targetCurrency, asOfDate);

        if (generalRate != null)
        {
            // Check if the general rate has an applicable tier for the transaction amount
            var applicableTier = generalRate.GetApplicableTier(transactionAmount);
            if (applicableTier != null)
            {
                return new ExchangeRateApplicationResult
                {
                    ExchangeRate = generalRate,
                    AppliedTier = applicableTier,
                    RateType = RateType.General,
                    IsTieredRate = true,
                    MinimumAmount = minimumAmount,
                    EffectiveRate = applicableTier.Rate,
                    EffectiveMargin = applicableTier.Margin
                };
            }

            // If no tier applies but we have a general rate, use the base general rate
            return new ExchangeRateApplicationResult
            {
                ExchangeRate = generalRate,
                AppliedTier = null,
                RateType = RateType.General,
                IsTieredRate = false,
                MinimumAmount = minimumAmount,
                EffectiveRate = generalRate.EffectiveRate,
                EffectiveMargin = generalRate.Margin
            };
        }

        // Fallback: No general rate found at all
        return new ExchangeRateApplicationResult
        {
            ExchangeRate = null,
            AppliedTier = null,
            RateType = RateType.General,
            IsTieredRate = false,
            MinimumAmount = minimumAmount,
            EffectiveRate = 0m,
            EffectiveMargin = 0m
        };
    }

    private async Task<ExchangeRate?> GetGeneralRateWithTiersAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime asOfDate)
    {
        return await DbSet
            .Include(er => er.Tiers)
            .Where(er => er.Type == RateType.General &&
                        er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .OrderByDescending(er => er.EffectiveFrom) // Get the most recent one
            .FirstOrDefaultAsync();
    }

    private async Task<List<ExchangeRate>> GetAllRatesWithTiersInHierarchyAsync(
        Guid? clientId,
        Guid? clientGroupId,
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime asOfDate)
    {
        var rates = new List<ExchangeRate>();

        // 1. Individual rates (highest priority)
        if (clientId.HasValue)
        {
            var individualRates = await DbSet
                .Include(er => er.Tiers)
                .Include(er => er.Client)
                .Where(er => er.ClientId == clientId.Value &&
                            er.BaseCurrency == baseCurrency &&
                            er.TargetCurrency == targetCurrency &&
                            er.IsActive &&
                            er.EffectiveFrom <= asOfDate &&
                            (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                .ToListAsync();
            rates.AddRange(individualRates);
        }

        // 2. Group rates (medium priority)
        if (clientGroupId.HasValue)
        {
            var groupRates = await DbSet
                .Include(er => er.Tiers)
                .Include(er => er.ClientGroup)
                .Where(er => er.Type == RateType.Group &&
                            er.ClientGroupId == clientGroupId.Value &&
                            er.BaseCurrency == baseCurrency &&
                            er.TargetCurrency == targetCurrency &&
                            er.IsActive &&
                            er.EffectiveFrom <= asOfDate &&
                            (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                .ToListAsync();
            rates.AddRange(groupRates);
        }

        // 3. General rates (lowest priority)
        var generalRates = await DbSet
            .Include(er => er.Tiers)
            .Where(er => er.Type == RateType.General &&
                        er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.IsActive &&
                        er.EffectiveFrom <= asOfDate &&
                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
            .ToListAsync();
        rates.AddRange(generalRates);

        // Return in hierarchy order: Individual > Group > General
        return rates.OrderByDescending(r => r.Type).ToList();
    }

    public async Task<Dictionary<Guid, ExchangeRate?>> GetApplicableExchangeRatesForClientsAsync(
    IEnumerable<Client> clients,
    Currency baseCurrency,
    Currency targetCurrency)
    {
        var clientList = clients.ToList();
        if (!clientList.Any())
            return new Dictionary<Guid, ExchangeRate?>();

        var now = DateTime.UtcNow;

        // Get ALL rates for the currency pair (including inactive for fallback)
        var allRates = await DbSet
            .Where(er => er.BaseCurrency == baseCurrency &&
                        er.TargetCurrency == targetCurrency &&
                        er.EffectiveFrom <= now)
            .ToListAsync();

        var result = new Dictionary<Guid, ExchangeRate?>();

        foreach (var client in clientList)
        {
            ExchangeRate? applicableRate = null;

            // 1. Try active individual rate
            var individualRate = allRates
                .Where(er => er.Type == RateType.Individual &&
                            er.ClientId == client.Id &&
                            er.IsActive &&
                            (er.EffectiveTo == null || er.EffectiveTo >= now))
                .OrderByDescending(er => er.EffectiveFrom)
                .FirstOrDefault();

            if (individualRate != null)
            {
                applicableRate = individualRate;
            }
            else
            {
                // 2. Try active group rate
                if (client.ClientGroupId.HasValue)
                {
                    var groupRate = allRates
                        .Where(er => er.Type == RateType.Group &&
                                    er.ClientGroupId == client.ClientGroupId.Value &&
                                    er.IsActive &&
                                    (er.EffectiveTo == null || er.EffectiveTo >= now))
                        .OrderByDescending(er => er.EffectiveFrom)
                        .FirstOrDefault();

                    if (groupRate != null)
                    {
                        applicableRate = groupRate;
                    }
                }

                // 3. Fallback to most recent general rate (even if expired)
                if (applicableRate == null)
                {
                    applicableRate = allRates
                        .Where(er => er.Type == RateType.General)
                        .OrderByDescending(er => er.EffectiveFrom)
                        .FirstOrDefault();
                }
            }

            result[client.Id] = applicableRate;
        }

        return result;
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetAllActiveRatesAsync(
        DateTime? asOfDate = null,
        Currency? baseCurrency = null,
        Currency? targetCurrency = null,
        RateType? rateType = null)
    {
        var effectiveDate = asOfDate ?? DateTime.UtcNow;

        var query = DbSet
            .Include(er => er.Client)
            .Include(er => er.ClientGroup)
            .Where(er => er.IsActive &&
                         er.EffectiveFrom <= effectiveDate &&
                         (er.EffectiveTo == null || er.EffectiveTo >= effectiveDate));

        // Apply optional filters using Currency objects
        if (baseCurrency != null)
        {
            query = query.Where(er => er.BaseCurrency == baseCurrency);
        }

        if (targetCurrency != null)
        {
            query = query.Where(er => er.TargetCurrency == targetCurrency);
        }

        if (rateType.HasValue)
        {
            query = query.Where(er => er.Type == rateType.Value);
        }

        // Try ordering by the Currency objects directly (if EF supports it)
        return await query
            .OrderBy(er => er.BaseCurrency)  // Use the Currency object directly
            .ThenBy(er => er.TargetCurrency) // Use the Currency object directly  
            .ThenByDescending(er => er.Type)
            .ThenByDescending(er => er.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<ExchangeRate?> GetApplicableRateAsync(Guid? clientId, Guid? clientGroupId, Currency baseCurrency,
        Currency targetCurrency, DateTime asOfDate)
    {
        try
        {
            // Get ALL rates (active and inactive) for historical fallback
            var allRates = await DbSet
                .Include(er => er.ClientGroup)
                .Include(er => er.Client)
                .Where(er => er.BaseCurrency == baseCurrency &&
                            er.TargetCurrency == targetCurrency &&
                            er.EffectiveFrom <= asOfDate)
                .OrderByDescending(er => er.Type)
                .ThenByDescending(er => er.EffectiveFrom)
                .ToListAsync();

            if (!allRates.Any())
                return null;

            // Priority order: Individual > Group > General
            // Check for active individual rate first
            if (clientId.HasValue)
            {
                var individualRate = allRates
                    .Where(er => er.Type == RateType.Individual &&
                                er.ClientId == clientId.Value &&
                                (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                    .OrderByDescending(er => er.EffectiveFrom)
                    .FirstOrDefault();

                if (individualRate != null && individualRate.IsActive)
                    return individualRate;
            }

            // Check for active group rate
            if (clientGroupId.HasValue)
            {
                var groupRate = allRates
                    .Where(er => er.Type == RateType.Group &&
                                er.ClientGroupId == clientGroupId.Value &&
                                (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
                    .OrderByDescending(er => er.EffectiveFrom)
                    .FirstOrDefault();

                if (groupRate != null && groupRate.IsActive)
                    return groupRate;
            }

            // Fallback to general rate - find the most recent one that was effective at or before asOfDate
            // This includes expired/inactive general rates as per requirement
            var generalRate = allRates
                .Where(er => er.Type == RateType.General)
                .OrderByDescending(er => er.EffectiveFrom)
                .FirstOrDefault(er => er.EffectiveFrom <= asOfDate);

            return generalRate;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    //public async Task<ExchangeRate?> GetApplicableRateAsync(
    //    Guid? clientId,
    //    Guid? clientGroupId,
    //    Currency baseCurrency,
    //    Currency targetCurrency,
    //    DateTime asOfDate)
    //{
    //    try
    //    {
    //        // Get all active rates for the currency pair that are effective at the given date
    //        var rates = await DbSet
    //            .Include(er => er.ClientGroupId)
    //            .Include(er => er.Client)
    //            .Where(er => er.BaseCurrency == baseCurrency &&
    //                        er.TargetCurrency == targetCurrency &&
    //                        er.IsActive &&
    //                        er.EffectiveFrom <= asOfDate &&
    //                        (er.EffectiveTo == null || er.EffectiveTo >= asOfDate))
    //            .OrderByDescending(er => er.Type) // Individual (3) > Group (2) > General (1)
    //            .ThenByDescending(er => er.EffectiveFrom) // Most recent first
    //            .ToListAsync();

    //        if (!rates.Any())
    //        {
    //            //_logger.LogWarning("No exchange rates found for {BaseCurrency} to {TargetCurrency} as of {AsOfDate}",
    //                //baseCurrency.Code, targetCurrency.Code, asOfDate);
    //            return null;
    //        }

    //        // Priority order: Individual > Group > General
    //        ExchangeRate? applicableRate;

    //        // 1. Check for individual rate for the specific client
    //        if (clientId.HasValue)
    //        {
    //            applicableRate = rates.FirstOrDefault(er =>
    //                er.Type == RateType.Individual && er.ClientId == clientId.Value);

    //            if (applicableRate != null)
    //            {
    //                //_logger.LogDebug("Found individual rate for client {ClientId}: {Rate}",
    //                //    clientId, applicableRate.EffectiveRate);
    //                return applicableRate;
    //            }
    //        }

    //        // 2. Check for group rate for the client's group
    //        if (clientGroupId.HasValue)
    //        {
    //            applicableRate = rates.FirstOrDefault(er =>
    //                er.Type == RateType.Group && er.ClientGroupId == clientGroupId.Value);

    //            if (applicableRate != null)
    //            {
    //                //_logger.LogDebug("Found group rate for group {GroupId}: {Rate}",
    //                //    clientGroupId, applicableRate.EffectiveRate);
    //                return applicableRate;
    //            }
    //        }

    //        // 3. Use general rate (applies to all clients)
    //        applicableRate = rates.FirstOrDefault(er => er.Type == RateType.General);

    //        if (applicableRate != null)
    //        {
    //            //_logger.LogDebug("Found general rate: {Rate}", applicableRate.EffectiveRate);
    //            return applicableRate;
    //        }

    //        //_logger.LogWarning("No applicable exchange rate found for client {ClientId}, group {GroupId}, {BaseCurrency} to {TargetCurrency}",
    //            //clientId, clientGroupId, baseCurrency.Code, targetCurrency.Code);

    //        return null;
    //    }
    //    catch (Exception ex)
    //    {
    //        //_logger.LogError(ex, "Error getting applicable exchange rate for client {ClientId}, group {GroupId}, {BaseCurrency} to {TargetCurrency}",
    //        //    clientId, clientGroupId, baseCurrency.Code, targetCurrency.Code);
    //        throw;
    //    }
    //}

    public async Task<RepositoryActionResult<ExchangeRate>> CreateGeneralExchangeRateAsync(
        CreateGeneralExchangeRateParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Check for overlapping general rates and expire them
            var overlappingRates = await DbSet
                .Where(er => er.Type == RateType.General &&
                            er.BaseCurrency == parameters.BaseCurrency &&
                            er.TargetCurrency == parameters.TargetCurrency &&
                            er.IsActive &&
                            (er.EffectiveTo == null || er.EffectiveTo >= parameters.EffectiveFrom))
                .ToListAsync();

            foreach (var overlappingRate in overlappingRates)
            {
                overlappingRate.Expire(parameters.EffectiveFrom, parameters.CreatedBy, "Replaced by new general rate");
            }

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

            // Add tiers if provided
            if (parameters.Tiers != null && parameters.Tiers.Any())
            {
                foreach (var tierRequest in parameters.Tiers)
                {
                    exchangeRate.AddTier(
                        tierRequest.MinAmount,
                        tierRequest.MaxAmount,
                        tierRequest.Rate,
                        tierRequest.Margin,
                        tierRequest.CreatedBy);
                }
            }

            DbSet.Add(exchangeRate);

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
            // Check for overlapping group rates for this specific client group
            var overlappingRates = await DbSet
                .Where(er => er.Type == RateType.Group &&
                            er.ClientGroupId == parameters.ClientGroupId &&
                            er.BaseCurrency == parameters.BaseCurrency &&
                            er.TargetCurrency == parameters.TargetCurrency &&
                            er.IsActive &&
                            (er.EffectiveTo == null || er.EffectiveTo >= parameters.EffectiveFrom))
                .ToListAsync();

            foreach (var overlappingRate in overlappingRates)
            {
                overlappingRate.Expire(parameters.EffectiveFrom, parameters.CreatedBy, "Replaced by new group rate");

                // REMOVED: History creation for expiration
                // var expireHistory = ExchangeRateHistory.CreateFromExchangeRate(...);
            }

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

            DbSet.Add(exchangeRate);

            // REMOVED: History creation for the creation
            // var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
            // await _exchangeRateHistoryRepository.AddAsync(history);

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

    //public async Task<RepositoryActionResult<ExchangeRate>> CreateGeneralExchangeRateAsync(
    //    CreateGeneralExchangeRateParameters parameters)
    //{
    //    await using var tx = await Context.Database.BeginTransactionAsync();
    //    try
    //    {
    //        // Use the domain factory method to create the exchange rate
    //        var exchangeRate = ExchangeRate.CreateGeneralRate(
    //            parameters.BaseCurrency,
    //            parameters.TargetCurrency,
    //            parameters.BaseCurrencyValue,
    //            parameters.TargetCurrencyValue,
    //            parameters.Margin,
    //            parameters.EffectiveFrom,
    //            parameters.CreatedBy,
    //            parameters.Source,
    //            parameters.EffectiveTo);

    //        // Add the exchange rate to the context
    //        DbSet.Add(exchangeRate);

    //        // Create history record for the creation
    //        var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
    //        await exchangeRateHistoryRepository.AddAsync(history);

    //        var result = await SaveChangesAsync();
    //        if (result > 0)
    //        {
    //            await tx.CommitAsync();
    //            return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
    //        }
    //        else
    //        {
    //            await tx.RollbackAsync();
    //            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
    //        }
    //    }
    //    catch (DbUpdateConcurrencyException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //    catch (Exception ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //}

    //public async Task<RepositoryActionResult<ExchangeRate>> CreateGroupExchangeRateAsync(
    //CreateGroupExchangeRateParameters parameters)
    //{
    //    await using var tx = await Context.Database.BeginTransactionAsync();
    //    try
    //    {
    //        // Use the domain factory method to create the exchange rate
    //        var exchangeRate = ExchangeRate.CreateGroupRate(
    //            parameters.BaseCurrency,
    //            parameters.TargetCurrency,
    //            parameters.BaseCurrencyValue,
    //            parameters.TargetCurrencyValue,
    //            parameters.Margin,
    //            parameters.ClientGroupId,
    //            parameters.EffectiveFrom,
    //            parameters.CreatedBy,
    //            parameters.Source,
    //            parameters.EffectiveTo);

    //        // Add the exchange rate to the context
    //        DbSet.Add(exchangeRate);

    //        // Create history record for the creation
    //        var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
    //        await exchangeRateHistoryRepository.AddAsync(history);

    //        var result = await SaveChangesAsync();
    //        if (result > 0)
    //        {
    //            await tx.CommitAsync();
    //            return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
    //        }
    //        else
    //        {
    //            await tx.RollbackAsync();
    //            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
    //        }
    //    }
    //    catch (DbUpdateConcurrencyException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //    catch (Exception ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //}

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

            // REMOVED: History creation for deactivation
            // var history = ExchangeRateHistory.CreateFromExchangeRate(...);

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

            // REMOVED: History creation for extension
            // var history = ExchangeRateHistory.CreateFromExchangeRate(...);

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
            // Check for overlapping individual rates for this specific client
            var overlappingRates = await DbSet
                .Where(er => er.Type == RateType.Individual &&
                            er.ClientId == parameters.ClientId &&
                            er.BaseCurrency == parameters.BaseCurrency &&
                            er.TargetCurrency == parameters.TargetCurrency &&
                            er.IsActive &&
                            (er.EffectiveTo == null || er.EffectiveTo >= parameters.EffectiveFrom))
                .ToListAsync();

            foreach (var overlappingRate in overlappingRates)
            {
                overlappingRate.Expire(parameters.EffectiveFrom, parameters.CreatedBy, "Replaced by new individual rate");

                // REMOVED: History creation for expiration
                // var expireHistory = ExchangeRateHistory.CreateFromExchangeRate(...);
            }

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

            DbSet.Add(exchangeRate);

            // REMOVED: History creation for the creation
            // var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
            // await _exchangeRateHistoryRepository.AddAsync(history);

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

    //public async Task<RepositoryActionResult<ExchangeRate>> CreateIndividualExchangeRateAsync(
    //CreateIndividualExchangeRateParameters parameters)
    //{
    //    await using var tx = await Context.Database.BeginTransactionAsync();
    //    try
    //    {
    //        // Use the domain factory method to create the exchange rate
    //        var exchangeRate = ExchangeRate.CreateIndividualRate(
    //            parameters.BaseCurrency,
    //            parameters.TargetCurrency,
    //            parameters.BaseCurrencyValue,
    //            parameters.TargetCurrencyValue,
    //            parameters.Margin,
    //            parameters.ClientId,
    //            parameters.EffectiveFrom,
    //            parameters.CreatedBy,
    //            parameters.Source,
    //            parameters.EffectiveTo);

    //        // Add the exchange rate to the context
    //        DbSet.Add(exchangeRate);

    //        // Create history record for the creation
    //        var history = ExchangeRateHistory.CreateForCreation(exchangeRate, parameters.CreatedBy);
    //        await exchangeRateHistoryRepository.AddAsync(history);

    //        var result = await SaveChangesAsync();
    //        if (result > 0)
    //        {
    //            await tx.CommitAsync();
    //            return new RepositoryActionResult<ExchangeRate>(exchangeRate, RepositoryActionStatus.Created);
    //        }
    //        else
    //        {
    //            await tx.RollbackAsync();
    //            return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.NothingModified);
    //        }
    //    }
    //    catch (DbUpdateConcurrencyException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
    //    }
    //    catch (DbUpdateException ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //    catch (Exception ex)
    //    {
    //        await tx.RollbackAsync();
    //        return new RepositoryActionResult<ExchangeRate>(null, RepositoryActionStatus.Error, ex);
    //    }
    //}

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

            // Update the exchange rate values
            exchangeRate.UpdateCurrencyValues(
                parameters.NewBaseCurrencyValue,
                parameters.NewTargetCurrencyValue,
                parameters.NewMargin);

            // REMOVED: History creation for update
            // var history = ExchangeRateHistory.CreateForUpdate(...);

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
            DateTime asOfDate)
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
            .FirstOrDefaultAsync();

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
                .FirstOrDefaultAsync();

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
            .FirstOrDefaultAsync();

        return generalRate;
    }

    public async Task MarkExpiredRatesAsInactiveAsync()
    {
        var now = DateTime.UtcNow;
        var expiredRates = await DbSet
            .Where(er => er.IsActive &&
                         er.EffectiveTo.HasValue &&
                         er.EffectiveTo < now)
            .ToListAsync();

        foreach (var rate in expiredRates)
        {
            rate.MarkAsHistorical();
        }

        await SaveChangesAsync();
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