using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.ClientGroups.Command;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ClientGroupRepository(IDatabaseFactory databaseFactory)
    : DataRepository<ClientGroup, Guid>(databaseFactory), IClientGroupRepository
{

    public async Task<RepositoryActionResult<ClientGroup>> DeactivateClientGroupAsync(
        DeactivateClientGroupParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var clientGroup = await DbSet.FirstOrDefaultAsync(cg => cg.Id == parameters.ClientGroupId);
            if (clientGroup == null)
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NotFound);

            // Deactivate the client group using domain method
            clientGroup.Deactivate(parameters.DeactivatedBy);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ClientGroup>(clientGroup, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ClientGroup>> ActivateClientGroupAsync(
        ActivateClientGroupParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var clientGroup = await DbSet.FirstOrDefaultAsync(cg => cg.Id == parameters.ClientGroupId);
            if (clientGroup == null)
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NotFound);

            // Activate the client group using domain method
            clientGroup.Activate(parameters.ActivatedBy);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ClientGroup>(clientGroup, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NothingModified);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ClientGroup>> CreateClientGroupAsync(
    CreateClientGroupParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            // Use the domain factory method to create the client group
            var clientGroup = ClientGroup.Create(
                parameters.Name,
                parameters.Description,
                parameters.CreatedBy);

            // Add the client group to the context
            DbSet.Add(clientGroup);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ClientGroup>(clientGroup, RepositoryActionStatus.Created);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NothingModified);
            }
        }
        //catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        //{
        //    await tx.RollbackAsync();
        //    return new RepositoryActionResult<ClientGroupId>(null, RepositoryActionStatus.Error,
        //        new Exception("A client group with this name already exists."));
        //}
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<RepositoryActionResult<ClientGroup>> UpdateClientGroupAsync(
        UpdateClientGroupParameters parameters)
    {
        await using var tx = await Context.Database.BeginTransactionAsync();
        try
        {
            var clientGroup = await DbSet.FirstOrDefaultAsync(cg => cg.Id == parameters.ClientGroupId);
            if (clientGroup == null)
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NotFound);

            // Update the client group using domain method
            clientGroup.Update(parameters.Name, parameters.Description, parameters.UpdatedBy);

            var result = await SaveChangesAsync();
            if (result > 0)
            {
                await tx.CommitAsync();
                return new RepositoryActionResult<ClientGroup>(clientGroup, RepositoryActionStatus.Updated);
            }
            else
            {
                await tx.RollbackAsync();
                return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.NothingModified);
            }
        }
        //catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        //{
        //    await tx.RollbackAsync();
        //    return new RepositoryActionResult<ClientGroupId>(null, RepositoryActionStatus.Error,
        //        new Exception("A client group with this name already exists."));
        //}
        catch (DbUpdateConcurrencyException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.ConcurrencyConflict, ex);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new RepositoryActionResult<ClientGroup>(null, RepositoryActionStatus.Error, ex);
        }
    }

    public async Task<ClientGroup?> GetByIdWithClientsAsync(Guid id) =>
        await DbSet
            .Include(cg => cg.Clients)
            .FirstOrDefaultAsync(cg => cg.Id == id);

    public async Task<PagedResult<ClientGroup>> GetAllAsync(
        bool? isActive = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (isActive.HasValue)
        {
            query = query.Where(cg => cg.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(cg =>
                cg.Name.ToLower().Contains(searchTerm) ||
                cg.Description.ToLower().Contains(searchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .OrderBy(cg => cg.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ClientGroup>
        {
            Items = items,
            Page = pageNumber,        // Changed from PageNumber to Page
            PageSize = pageSize,
            TotalCount = totalCount
            // TotalPages, HasPrevious, HasNext are computed properties
        };
    }

    public async Task<IReadOnlyList<ClientGroup>> GetAllWithoutPaginationAsync(
        bool? isActive = null,
        string? searchTerm = null)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (isActive.HasValue)
        {
            query = query.Where(cg => cg.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(cg =>
                cg.Name.ToLower().Contains(searchTerm) ||
                cg.Description.ToLower().Contains(searchTerm));
        }

        // Return all results without pagination
        return await query
            .OrderBy(cg => cg.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClientGroup>> GetByStatusAsync(bool isActive) =>
        await DbSet
            .Where(cg => cg.IsActive == isActive)
            .OrderBy(cg => cg.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<ClientGroup>> GetAllActiveAsync() =>
        await DbSet
            .Where(cg => cg.IsActive)
            .OrderBy(cg => cg.Name)
            .ToListAsync();
}