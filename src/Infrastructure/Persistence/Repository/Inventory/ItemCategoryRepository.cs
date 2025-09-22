using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ItemCategoryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ItemCategory, string>(databaseFactory), IItemCategoryRepository
{
    public override async Task<RepositoryActionResult<ItemCategory>> AddAsync(ItemCategory itemCategory)
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
            itemCategory.SetId(newId);

            await DbSet.AddAsync(itemCategory);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<ItemCategory>(itemCategory, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ItemCategory>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<ItemCategory>> UpdateAsyncAsync(Guid publicId,
        ItemCategory itemCategory)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<ItemCategory>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(itemCategory))
                return new RepositoryActionResult<ItemCategory>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(itemCategory);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<ItemCategory>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<ItemCategory>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<ItemCategory>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<ItemCategory>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ItemCategory>(null, RepositoryActionStatus.Error, ex);
        }
    }
}