using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Clients.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ClientRepository(
    UserManager<ApplicationUser> userManager,
    IClientGroupRepository clientGroupRepository,
    IDatabaseFactory databaseFactory)
    : DataRepository<Client, Guid>(databaseFactory), IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetClientsForExchangeRateQueryAsync() =>
        await DbSet
            .Include(c => c.Wallet)
            .Include(c => c.ClientGroup)
            .Where(c => c.Status == ClientStatus.Active)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

    public async Task<Client?> GetClientForExchangeRateQueryAsync(Guid clientId) =>
        await DbSet
            .Include(c => c.Wallet)
            .Include(c => c.ClientGroup)
            .Where(c => c.Status == ClientStatus.Active)
            .FirstOrDefaultAsync(x => x.Id == clientId);

    public async Task<Client?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(s => s.Email == email);

    public async Task<Client?> GetByUserIdAsync(Guid userId) =>
        await DbSet.FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task<RepositoryActionResult<Client>> RegisterClientAsync(RegisterClientParameters parameters)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            parameters.Validate();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = parameters.Email,
                Email = parameters.Email,
                PhoneNumber = parameters.PhoneNumber,
                FirstName = parameters.FirstName,
                LastName = parameters.LastName,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = parameters.CreatedBy
            };

            var identityResult = await userManager.CreateAsync(user, parameters.Password);

            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error);
            }

            var client = Client.Create(parameters.Email, parameters.PhoneNumber, parameters.FirstName,
                parameters.LastName);

            client.LinkToUser(user.Id);

            var result = await AddAsync(client);
            if (result.Status != RepositoryActionStatus.Created)
            {
                await transaction.RollbackAsync();
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error);
            }

            user.ClientId = client.Id;

            identityResult = await userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error);
            }

            await transaction.CommitAsync();

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Client>> RemoveFromGroupAsync(Guid clientId,
        RemoveFromGroupParameters parameters)
    {
        try
        {
            var existingEntity = await GetAsync(clientId);
            if (existingEntity == null)
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            existingEntity.RemoveFromGroup(parameters.Reason);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Client>> AssignToGroupAsync(Guid clientId,
        AssignToGroupParameters parameters)
    {
        try
        {
            var existingEntity = await GetAsync(clientId);
            if (existingEntity == null)
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            var clientGroup = await clientGroupRepository.GetAsync(parameters.ClientGroupId);
            if (clientGroup == null)
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Invalid,"Client group does not exist, operation cancelled!");

            existingEntity.AssignToGroup(clientGroup, parameters.Reason);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<Client>> UpdateGroupAsync(Guid publicId,
        UpdateGroupParameters parameters)
    {
        try
        {
            var existingEntity = await GetAsync(publicId);
            if (existingEntity == null)
                return new RepositoryActionResult<Client>(null, RepositoryActionStatus.NotFound);

            var entry = Context.Entry(existingEntity);

            if (entry.State == EntityState.Detached)
                DbSet.Attach(existingEntity);

            ClientGroup? clientGroup = null;

            // If ClientGroupId is provided, fetch the ClientGroupId entity
            if (!string.IsNullOrWhiteSpace(parameters.ClientGroupId))
            {
                if (!Guid.TryParse(parameters.ClientGroupId, out var clientGroupId))
                    return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error);

                clientGroup = await clientGroupRepository.GetAsync(clientGroupId);
                if (clientGroup == null)
                    return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error);
            }

            // Apply domain logic - UpdateGroup handles both assignment and removal
            existingEntity.UpdateGroup(clientGroup, parameters.Reason);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await entry.ReloadAsync();
                return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.Updated);
            }

            return new RepositoryActionResult<Client>(existingEntity, RepositoryActionStatus.NothingModified);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            return new RepositoryActionResult<Client>(null, RepositoryActionStatus.Error, ex);
        }
    }
}