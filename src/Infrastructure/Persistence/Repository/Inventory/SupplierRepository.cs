using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Repository.Inventory;

public class SupplierRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Supplier, string>(databaseFactory), ISupplierRepository
{
    public override async Task<RepositoryActionResult<Supplier>> AddAsync(Supplier supplier)
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
            supplier.SetId(newId);

            await DbSet.AddAsync(supplier);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Supplier>(supplier, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Supplier>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Supplier>> UpdateAsyncAsync(Guid publicId,
        Supplier supplier)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Supplier>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(supplier))
                return new RepositoryActionResult<Supplier>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(supplier);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Supplier>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Supplier>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Supplier>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Supplier>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Supplier>(null, RepositoryActionStatus.Error, ex);
        }
    }
}