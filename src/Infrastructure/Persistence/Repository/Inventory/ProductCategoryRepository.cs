using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ProductCategoryRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ProductCategory, string>(databaseFactory), IProductCategoryRepository
{
    public override async Task<RepositoryActionResult<ProductCategory>> AddAsync(ProductCategory productCategory)
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
            productCategory.SetId(newId);

            await DbSet.AddAsync(productCategory);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<ProductCategory>(productCategory, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ProductCategory>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<ProductCategory>> UpdateAsyncAsync(Guid publicId,
        ProductCategory productCategory)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<ProductCategory>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(productCategory))
                return new RepositoryActionResult<ProductCategory>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(productCategory);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<ProductCategory>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<ProductCategory>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<ProductCategory>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<ProductCategory>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ProductCategory>(null, RepositoryActionStatus.Error, ex);
        }
    }
}