using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ProductMovementRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ProductMovement, string>(databaseFactory), IProductMovementRepository
{
    public override async Task<RepositoryActionResult<ProductMovement>> AddAsync(ProductMovement productMovement)
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

            var newId = (lastNumber + 1).ToString(CultureInfo.InvariantCulture).PadLeft(3,'0');
            productMovement.SetId(newId);

            await DbSet.AddAsync(productMovement);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<ProductMovement>(productMovement, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ProductMovement>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<IEnumerable<ProductMovement>>> UpdateItemMovementsAsync(string id,
        ProductMovement[] itemMovements)
    {
        await DeleteManyAsync(x => x.Id == id);

        if (itemMovements.Length == 0)
        {
            return new RepositoryActionResult<IEnumerable<ProductMovement>>(itemMovements,
                RepositoryActionStatus.Okay);
        }

        var result = await AddManyAsync(itemMovements);
        if (result.Status == RepositoryActionStatus.Created)
        {
            return new RepositoryActionResult<IEnumerable<ProductMovement>>(result.Entity,
                RepositoryActionStatus.Okay);
        }

        return result;
    }
}