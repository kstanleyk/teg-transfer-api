using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Sales;
using Agrovet.Domain.Entity.Sales;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class DistributionChannelRepository(IDatabaseFactory databaseFactory)
    : DataRepository<DistributionChannel, string>(databaseFactory), IDistributionChannelRepository
{
    public override async Task<RepositoryActionResult<DistributionChannel>> AddAsync(DistributionChannel distributionChannel)
    {
        try
        {
            var lastIdValue = await DbSet
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var lastNumber = string.IsNullOrWhiteSpace(lastIdValue)
                ? 0
                : lastIdValue.ToNumValue();

            var newId = (lastNumber + 1).ToString(CultureInfo.InvariantCulture).PadLeft(2,'0');
            distributionChannel.SetId(newId);

            await DbSet.AddAsync(distributionChannel);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<DistributionChannel>(distributionChannel, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<DistributionChannel>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<DistributionChannel>> UpdateAsyncAsync(Guid publicId,
        DistributionChannel distributionChannel)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<DistributionChannel>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(distributionChannel))
                return new RepositoryActionResult<DistributionChannel>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(distributionChannel);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<DistributionChannel>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<DistributionChannel>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<DistributionChannel>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<DistributionChannel>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<DistributionChannel>(null, RepositoryActionStatus.Error, ex);
        }
    }
}