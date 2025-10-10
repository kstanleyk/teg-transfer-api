using Microsoft.EntityFrameworkCore;
using Transfer.Application.Helpers;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Infrastructure.Persistence.Repository.Inventory;

public class OrderRepository(IDatabaseFactory databaseFactory, IOrderDetailRepository orderDetailRepository,
    IProductMovementRepository productMovementRepository) : DataRepository<Order, string>(databaseFactory), IOrderRepository
{
    public override async Task<RepositoryActionResult<Order>> AddAsync(Order order)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var orderId = await GenerateReferenceAsync(x => x.Id, order.OrderDate);
            order.SetId(orderId);

            await DbSet.AddAsync(order);

            var result = await SaveChangesAsync();
            if (result == 0)
                return new RepositoryActionResult<Order>(order, RepositoryActionStatus.NothingModified);

            await tx.CommitAsync();
            return new RepositoryActionResult<Order>(order, RepositoryActionStatus.Created);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Order>> UpdateAsyncAsync(Guid publicId, Order order)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (existingEntity.HasChanges(order))
            {
                var entry = Context.Entry(existingEntity);

                if (entry.State == EntityState.Detached)
                    DbSet.Attach(existingEntity);

                existingEntity.Update(order);

                var result = await SaveChangesAsync();
                if (result > 0)
                {
                    await entry.ReloadAsync();
                }
            }

            var orderDetails = order.OrderDetails.ToArray();
            var orderId = existingEntity.Id;

            if (orderDetails.Length > 0)
            {
                var orderDetailResult = await orderDetailRepository.UpdateOrderDetailsAsync(orderId, orderDetails);
                if (orderDetailResult.Status != RepositoryActionStatus.Okay)
                {
                    await tx.RollbackAsync();
                    return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NothingModified);
                }
            }
            await tx.CommitAsync();
            return new RepositoryActionResult<Order>(existingEntity, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Order>> ReceiveOrderAsync(Guid publicId, Order order)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (existingEntity.HasChanges(order))
            {
                var entry = Context.Entry(existingEntity);

                if (entry.State == EntityState.Detached)
                    DbSet.Attach(existingEntity);

                existingEntity.Update(order);

                var result = await SaveChangesAsync();
                if (result > 0)
                {
                    await entry.ReloadAsync();
                }
            }

            var orderDetails = order.OrderDetails.ToArray();
            var orderId = existingEntity.Id;

            if (orderDetails.Length > 0)
            {
                var orderDetailResult = await orderDetailRepository.UpdateOrderDetailsAsync(orderId, orderDetails);
                if (orderDetailResult.Status != RepositoryActionStatus.Okay)
                {
                    await tx.RollbackAsync();
                    return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NothingModified);
                }

                var movementId =
                    await productMovementRepository.GenerateReferenceAsync(im => im.Id, order.TransDate);

                order.UpdateItemMovements(movementId);

                var itemMovementResults =
                    await productMovementRepository.UpdateItemMovementsAsync(orderId, order.ItemMovements.ToArray());
                if (itemMovementResults.Status != RepositoryActionStatus.Okay)
                {
                    await tx.RollbackAsync();
                    return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NothingModified);
                }
            }
            await tx.CommitAsync();
            return new RepositoryActionResult<Order>(existingEntity, RepositoryActionStatus.Updated);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Order>> EditAsync(Order order)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var orderDetails = order.OrderDetails.ToArray();

            var orderId = await GenerateReferenceAsync(x => x.Id, order.OrderDate);
            order.SetId(orderId);

            await DbSet.AddAsync(order);

            var result = await SaveChangesAsync();
            if (result == 0)
                return new RepositoryActionResult<Order>(order, RepositoryActionStatus.NothingModified);

            if (orderDetails.Length > 0)
            {
                var orderDetailResult = await orderDetailRepository.UpdateOrderDetailsAsync(orderId, orderDetails);
                if (orderDetailResult.Status != RepositoryActionStatus.Okay)
                {
                    await tx.RollbackAsync();
                    return new RepositoryActionResult<Order>(null, RepositoryActionStatus.NothingModified);
                }
            }

            await tx.CommitAsync();
            return new RepositoryActionResult<Order>(order, RepositoryActionStatus.Created);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<Order>(null, RepositoryActionStatus.Error, ex);
        }
    }

    protected override void DisposeCore()
    {
        orderDetailRepository.Dispose();
        productMovementRepository.Dispose();
    }
}