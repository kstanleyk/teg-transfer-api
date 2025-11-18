using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class MinimumAmountConfigurationRepository(IDatabaseFactory databaseFactory)
    : DataRepository<MinimumAmountConfiguration, Guid>(databaseFactory), IMinimumAmountConfigurationRepository
{
    public async Task<IReadOnlyList<MinimumAmountConfiguration>> GetOverlappingConfigurationsAsync(
        Currency baseCurrency,
        Currency targetCurrency,
        DateTime effectiveFrom,
        DateTime? effectiveTo)
    {
        return await DbSet
            .Where(mac => mac.BaseCurrency == baseCurrency &&
                         mac.TargetCurrency == targetCurrency &&
                         mac.IsActive &&
                         ((mac.EffectiveFrom <= effectiveFrom && (mac.EffectiveTo == null || mac.EffectiveTo >= effectiveFrom)) ||
                          (effectiveTo != null && mac.EffectiveFrom <= effectiveTo && (mac.EffectiveTo == null || mac.EffectiveTo >= effectiveTo)) ||
                          (mac.EffectiveFrom >= effectiveFrom && (effectiveTo == null || mac.EffectiveFrom <= effectiveTo))))
            .ToListAsync();
    }

    public async Task<RepositoryActionResult<MinimumAmountConfiguration>> CreateAsync(
        CreateMinimumAmountConfigurationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Use the domain factory method to create the configuration
            var configuration = MinimumAmountConfiguration.Create(
                parameters.BaseCurrency,
                parameters.TargetCurrency,
                parameters.MinimumAmount,
                parameters.EffectiveFrom,
                parameters.CreatedBy,
                parameters.EffectiveTo);

            // Add the configuration to the context
            DbSet.Add(configuration);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(configuration, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.NothingModified);
            }
        }
        //catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        //{
        //    await tx.RollbackAsync();
        //    return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error,
        //        new Exception("A minimum amount configuration already exists for this currency pair during the specified period"));
        //}
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<MinimumAmountConfiguration>> UpdateAsync(
        UpdateMinimumAmountConfigurationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var configuration = await DbSet
                .FirstOrDefaultAsync(mac => mac.Id == parameters.ConfigurationId);

            if (configuration == null)
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.NotFound);

            if (!configuration.IsActive)
                return new RepositoryActionResult<MinimumAmountConfiguration>(configuration, RepositoryActionStatus.Error,
                    new Exception("Cannot update an inactive minimum amount configuration"));

            // Check for overlapping configurations (excluding the current one)
            var overlappingConfigs = await DbSet
                .Where(mac => mac.Id != parameters.ConfigurationId &&
                             mac.BaseCurrency == configuration.BaseCurrency &&
                             mac.TargetCurrency == configuration.TargetCurrency &&
                             mac.IsActive &&
                             ((mac.EffectiveFrom <= parameters.EffectiveFrom && (mac.EffectiveTo == null || mac.EffectiveTo >= parameters.EffectiveFrom)) ||
                              (parameters.EffectiveTo != null && mac.EffectiveFrom <= parameters.EffectiveTo && (mac.EffectiveTo == null || mac.EffectiveTo >= parameters.EffectiveTo)) ||
                              (mac.EffectiveFrom >= parameters.EffectiveFrom && (parameters.EffectiveTo == null || mac.EffectiveFrom <= parameters.EffectiveTo))))
                .ToListAsync();

            if (overlappingConfigs.Any())
            {
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error,
                    new Exception("Another active minimum amount configuration exists for this currency pair during the specified period"));
            }

            // Update the configuration
            configuration.Update(
                parameters.MinimumAmount,
                parameters.EffectiveFrom,
                parameters.EffectiveTo);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(configuration, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<MinimumAmountConfiguration>> DeactivateAsync(
        DeactivateMinimumAmountConfigurationParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var configuration = await DbSet
                .FirstOrDefaultAsync(mac => mac.Id == parameters.ConfigurationId);

            if (configuration == null)
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.NotFound);

            if (!configuration.IsActive)
                return new RepositoryActionResult<MinimumAmountConfiguration>(configuration, RepositoryActionStatus.NothingModified);

            // Deactivate the configuration
            configuration.Deactivate();

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(configuration, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<MinimumAmountConfiguration>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<MinimumAmountConfiguration?> GetApplicableMinimumAmountAsync(
        Currency? baseCurrency,
        Currency? targetCurrency,
        DateTime asOfDate)
    {
        return await DbSet
            .Where(mac => mac.BaseCurrency == baseCurrency &&
                         mac.TargetCurrency == targetCurrency &&
                         mac.IsActive &&
                         mac.EffectiveFrom <= asOfDate &&
                         (mac.EffectiveTo == null || mac.EffectiveTo >= asOfDate))
            .OrderByDescending(mac => mac.EffectiveFrom) // Get the most recent one
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<MinimumAmountConfiguration>> GetActiveConfigurationsAsync(
        Currency? baseCurrency = null,
        Currency? targetCurrency = null,
        DateTime? asOfDate = null)
    {
        var query = DbSet.Where(mac => mac.IsActive);

        if (baseCurrency != null)
        {
            query = query.Where(mac => mac.BaseCurrency == baseCurrency);
        }

        if (targetCurrency != null)
        {
            query = query.Where(mac => mac.TargetCurrency == targetCurrency);
        }

        if (asOfDate.HasValue)
        {
            query = query.Where(mac => mac.EffectiveFrom <= asOfDate.Value &&
                                       (mac.EffectiveTo == null || mac.EffectiveTo >= asOfDate.Value));
        }

        // Execute the query first, then apply ordering in memory
        var results = await query.ToListAsync();

        // Apply ordering in memory (since EF struggles with value object property access)
        return results
            .OrderBy(mac => mac.BaseCurrency.Code)
            .ThenBy(mac => mac.TargetCurrency.Code)
            .ThenBy(mac => mac.EffectiveFrom)
            .ToList();
    }

    public async Task<MinimumAmountConfiguration?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .FirstOrDefaultAsync(mac => mac.Id == id);
    }

    // Helper method to check for unique constraint violations
    //private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    //{
    //    return ex.InnerException is SqlException sqlException &&
    //           (sqlException.Number == 2601 || sqlException.Number == 2627); // Unique constraint violation
    //}
}