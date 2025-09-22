using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Inventory;

public class ItemMovementRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ItemMovement, string>(databaseFactory), IItemMovementRepository
{
    public override async Task<RepositoryActionResult<ItemMovement>> AddAsync(ItemMovement itemMovement)
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
            itemMovement.SetId(newId);

            await DbSet.AddAsync(itemMovement);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<ItemMovement>(itemMovement, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<ItemMovement>(null, RepositoryActionStatus.Error, ex);
        }
    }
}