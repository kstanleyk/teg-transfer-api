using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ClientRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Client, Guid>(databaseFactory), IClientRepository
{
    public async Task<Client?> GetByEmailAsync(string email) => 
        await DbSet.FirstOrDefaultAsync(x => x.Email == email);
}