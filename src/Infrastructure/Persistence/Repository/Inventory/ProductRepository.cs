using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Agrovet.Application.Features.Inventory.Product.Dtos;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ProductRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Product, string>(databaseFactory), IProductRepository
{
    public override async Task<RepositoryActionResult<Product>> AddAsync(Product product)
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
            product.SetId(newId);

            await DbSet.AddAsync(product);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Product>(product, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Product>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public override async Task<RepositoryActionResult<Product>> UpdateAsyncAsync(Guid publicId, Product product)
    {
        try
        {
            var existingEntity = await GetByPublicIdAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Product>(null, RepositoryActionStatus.NotFound);

            // Return early if no changes
            if (!existingEntity.HasChanges(product))
                return new RepositoryActionResult<Product>(existingEntity, RepositoryActionStatus.NothingModified);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.Update(product);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Product>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Product>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Product>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Product>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Product>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<ProductStockBalanceResponse[]> GetItemStockBalancesAsync()
    {
        try
        {
            const string selectSql = """

                                     WITH item_movements AS (
                                         SELECT 
                                             im.source_id, 
                                             im.source_line_num, 
                                             coalesce(SUM(im.qtty) FILTER (WHERE im.sense = 'D'),0) AS total_debit,
                                             coalesce(SUM(im.qtty) FILTER (WHERE im.sense = 'C'),0) AS total_credit
                                         FROM inventory.item_movement im
                                         GROUP BY im.source_id, im.source_line_num
                                     )
                                     SELECT 
                                         od.id, 
                                         od.line_num as lineNum, 
                                     	od.item,
                                     	od.unit_cost as unitcost,
                                         COALESCE(im.total_debit, 0) - COALESCE(im.total_credit, 0) AS stockQtty
                                     FROM inventory.order_detail od
                                     LEFT JOIN item_movements im 
                                         ON im.source_id = od.id 
                                         AND im.source_line_num = od.line_num
                                     WHERE COALESCE(im.total_debit, 0) - COALESCE(im.total_credit, 0) <> 0;
                                     """;

            var stockDetails = await Db.QueryAsync<ProductStockBalanceResponse>(selectSql);

            return stockDetails.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}