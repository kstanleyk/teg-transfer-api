using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Core;

public class EstateRepository(IDatabaseFactory databaseFactory)
    : Repository<Estate, string>(databaseFactory), IEstateRepository
{
    public override async Task<RepositoryActionResult<Estate>> AddAsync(Estate department)
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
            department.SetId(newId);

            await DbSet.AddAsync(department);
            var changes = await SaveChangesAsync();

            var status = changes == 0
                ? RepositoryActionStatus.NothingModified
                : RepositoryActionStatus.Created;

            return new RepositoryActionResult<Estate>(department, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Estate>(null, RepositoryActionStatus.Error, ex);
        }
    }
}