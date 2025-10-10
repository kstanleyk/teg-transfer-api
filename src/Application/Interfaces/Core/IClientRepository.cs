using Transfer.Domain.Entity.Core;

namespace Transfer.Application.Interfaces.Core;

public interface IClientRepository : IRepository<Client, Guid>
{
    Task<Client?> GetByEmailAsync(string email);
}