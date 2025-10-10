using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Repository.Inventory;

public class OrderTypeRepository(IDatabaseFactory databaseFactory)
    : DataRepository<OrderType, string>(databaseFactory), IOrderTypeRepository
{
    public override async Task<RepositoryActionResult<OrderType>> AddAsync(OrderType orderType)
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
            orderType.SetId(newId);

            await DbSet.AddAsync(orderType);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<OrderType>(orderType, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<OrderType>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<OrderType>> UpdateAsyncAsync(Guid publicId,
        OrderType orderType)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<OrderType>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(orderType))
                return new RepositoryActionResult<OrderType>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(orderType);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<OrderType>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<OrderType>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<OrderType>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<OrderType>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<OrderType>(null, RepositoryActionStatus.Error, ex);
        }
    }
}