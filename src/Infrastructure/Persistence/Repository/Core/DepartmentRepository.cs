using System.Globalization;
using Agrovet.Application.Helpers;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Wrappers;
using Agrovet.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Repository.Core;

public class DepartmentRepository(IDatabaseFactory databaseFactory)
    : Repository<Department, string>(databaseFactory), IDepartmentRepository
{
    public override async Task<RepositoryActionResult<Department>> AddAsync(Department department)
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

            return new RepositoryActionResult<Department>(department, status);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Department>(null, RepositoryActionStatus.Error, ex);
        }
    }
}