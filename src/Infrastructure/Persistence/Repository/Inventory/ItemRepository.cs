using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ItemRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Item, string>(databaseFactory), IItemRepository
{
    public override async Task<RepositoryActionResult<Item>> AddAsync(Item item)
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

            var newId = (lastNumber + 1).ToString(CultureInfo.InvariantCulture).PadLeft(6, '0');
            item.SetId(newId);

            await DbSet.AddAsync(item);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Item>(item, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Item>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Item>> UpdateAsyncAsync(Guid publicId, Item item)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Item>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(item))
                return new RepositoryActionResult<Item>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(item);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Item>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Item>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Item>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Item>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Item>(null, RepositoryActionStatus.Error, ex);
        }
    }
}