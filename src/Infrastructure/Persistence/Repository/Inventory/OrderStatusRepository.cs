using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class OrderStatusRepository(IDatabaseFactory databaseFactory)
    : DataRepository<OrderStatus, string>(databaseFactory), IOrderStatusRepository
{
    public override async Task<RepositoryActionResult<OrderStatus>> AddAsync(OrderStatus ordetStatus)
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
            ordetStatus.SetId(newId);

            await DbSet.AddAsync(ordetStatus);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<OrderStatus>(ordetStatus, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<OrderStatus>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<OrderStatus>> UpdateAsyncAsync(Guid publicId,
        OrderStatus ordetStatus)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<OrderStatus>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(ordetStatus))
                return new RepositoryActionResult<OrderStatus>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(ordetStatus);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<OrderStatus>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<OrderStatus>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<OrderStatus>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<OrderStatus>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<OrderStatus>(null, RepositoryActionStatus.Error, ex);
        }
    }
}