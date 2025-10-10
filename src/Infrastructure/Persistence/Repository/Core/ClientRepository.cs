using Microsoft.EntityFrameworkCore;
using Transfer.Application.Interfaces.Core;
using Transfer.Domain.Entity.Core;

namespace Transfer.Infrastructure.Persistence.Repository.Core;

public class ClientRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Client, Guid>(databaseFactory), IClientRepository
{
    public async Task<Client?> GetByEmailAsync(string email) => 
        await DbSet.FirstOrDefaultAsync(x => x.Email == email);
}