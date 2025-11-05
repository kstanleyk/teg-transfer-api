using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ClientGroupRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ClientGroup, Guid>(databaseFactory), IClientGroupRepository
{

}