using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.Entity.Inventory;
using Microsoft.EntityFrameworkCore;

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
}