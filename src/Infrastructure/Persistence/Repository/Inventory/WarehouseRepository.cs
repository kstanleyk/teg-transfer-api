using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Core;

namespace Transfer.Infrastructure.Persistence.Repository.Inventory;

public class WarehouseRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Warehouse, string>(databaseFactory), IWarehouseRepository
{
    public override async Task<RepositoryActionResult<Warehouse>> AddAsync(Warehouse warehouse)
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
            warehouse.SetId(newId);

            await DbSet.AddAsync(warehouse);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Warehouse>(warehouse, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Warehouse>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Warehouse>> UpdateAsyncAsync(Guid publicId,
        Warehouse warehouse)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Warehouse>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(warehouse))
                return new RepositoryActionResult<Warehouse>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(warehouse);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Warehouse>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Warehouse>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Warehouse>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Warehouse>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Warehouse>(null, RepositoryActionStatus.Error, ex);
        }
    }
}