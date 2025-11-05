using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Authorization;
using TegWallet.Domain.Entity.Auth;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Infrastructure.Persistence.Context;

public class TegWalletDatabaseSeeder(TegWalletContext context, UserManager<Client> userManager)
{
    public async Task SeedDatabaseAsync()
    {
        await CheckAndApplyPendingMigrationAsync();

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();

        await SeedExchangeRatesAndGroupsAsync();
    }

    private async Task CheckAndApplyPendingMigrationAsync()
    {
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        var existingRoleNames = await context.RoleSet
            .Select(r => r.Name)
            .ToListAsync();

        // Step 1: Add missing roles
        var defaultRoles = AppRoles.DefaultRoles
            .Where(name => !existingRoleNames.Contains(name))
            .Select(Role.Create)
            .ToList();

        if (defaultRoles.Any())
        {
            await context.RoleSet.AddRangeAsync(defaultRoles);
            await context.SaveChangesAsync();
        }

        // Step 2: Fetch all roles and permissions
        var roles = await context.RoleSet.ToListAsync();
        var permissions = await context.PermissionSet.ToListAsync();

        var rolePermissionsToAdd = new List<RolePermission>();

        foreach (var role in roles)
        {
            // Check if role already has permissions
            var existingRolePermissionIds = await context.RolePermissionSet
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var relevantPermissions = role.Name switch
            {
                AppRoles.Admin => permissions, // all permissions
                AppRoles.Basic => permissions.Where(p => p.IsBasic).ToList(), // only basic
                _ => []
            };

            var newPermissions = relevantPermissions
                .Where(p => !existingRolePermissionIds.Contains(p.Id))
                .Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id,
                    CreatedOn = DateTime.UtcNow
                });

            rolePermissionsToAdd.AddRange(newPermissions);
        }

        if (rolePermissionsToAdd.Any())
        {
            context.RolePermissionSet.AddRange(rolePermissionsToAdd);
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedPermissionsAsync()
    {
        // 1. Get existing permission names from the DB
        var existingPermissionNames = await context.PermissionSet
            .Select(p => p.Id)
            .ToListAsync();

        // 2. Identify new permissions to add
        var newPermissions = AppPermissions.AllPermissions
            .Where(p => !existingPermissionNames.Contains(p.Name))
            .Select(p => Permission.Create(p.Name, p.Feature, p.Action, p.Group, p.Description, p.IsBasic))
            .ToList();

        // 3. Add new permissions
        if (newPermissions.Any())
        {
            context.PermissionSet.AddRange(newPermissions);
            await context.SaveChangesAsync();
        }

        // 4. Ensure Admin role has all permissions
        var adminRole = await context.RoleSet
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == AppRoles.Admin);

        if (adminRole != null)
        {
            var allPermissionIds = await context.PermissionSet
                .Select(p => p.Id)
                .ToListAsync();

            var existingAdminPermissionIds = adminRole.RolePermissions
                .Select(rp => rp.PermissionId)
                .ToHashSet();

            var missingPermissionIds = allPermissionIds
                .Where(pid => !existingAdminPermissionIds.Contains(pid))
                .ToList();

            foreach (var pid in missingPermissionIds)
            {
                context.RolePermissionSet.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = pid,
                    CreatedOn = DateTime.UtcNow
                });
            }

            if (missingPermissionIds.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        var roles = await context.RoleSet.ToListAsync();
        var roleByName = roles.ToDictionary(r => r.Name);

        var defaultUsers = new[]
        {
        new
        {
            IdentityId = AppCredentials.AdminIdentityId,
            Email = AppCredentials.AdminEmail,
            FullName = AppCredentials.AdminUserName,
            RoleName = AppRoles.Admin,
            AppCredentials.ImageUrl
        },
        new
        {
            IdentityId = AppCredentials.BasicIdentityId,
            Email = AppCredentials.BasicEmail,
            FullName = AppCredentials.BasicUserName,
            RoleName = AppRoles.Basic,
            AppCredentials.ImageUrl
        }
    };

        foreach (var user in defaultUsers)
        {
            var existingUser = await context.UserSet
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.IdentityId == user.IdentityId);

            if (existingUser == null)
            {
                existingUser = User.Create(
                    id: Guid.NewGuid(),
                    identityId: user.IdentityId,
                    email: user.Email,
                    fullName: user.FullName,
                    profileImageUrl: user.ImageUrl
                );

                context.UserSet.Add(existingUser);
                await context.SaveChangesAsync(); // Save to generate ID
            }

            // Assign roles
            if (user.RoleName == AppRoles.Admin)
            {
                // Ensure Admin has all roles
                var assignedRoleIds = existingUser.UserRoles.Select(ur => ur.RoleId).ToHashSet();

                foreach (var role in roles)
                {
                    if (!assignedRoleIds.Contains(role.Id))
                    {
                        context.UserRoleSet.Add(new UserRole
                        {
                            UserId = existingUser.Id,
                            RoleId = role.Id,
                            CreatedOn = DateTime.UtcNow
                        });
                    }
                }
            }
            else
            {
                // Assign only the specified role
                var role = roleByName[user.RoleName];
                var hasRole = existingUser.UserRoles.Any(r => r.RoleId == role.Id);

                if (!hasRole)
                {
                    context.UserRoleSet.Add(new UserRole
                    {
                        UserId = existingUser.Id,
                        RoleId = role.Id,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedExchangeRatesAndGroupsAsync()
    {
        // 1. Seed Client Groups
        if (!await context.ClientGroupSet.AnyAsync())
        {
            var clientGroups = new[]
            {
            ClientGroup.Create("VIP", "VIP clients with premium exchange rates", "SYSTEM"),
            ClientGroup.Create("Corporate", "Corporate clients with business rates", "SYSTEM"),
            ClientGroup.Create("Retail", "Retail clients with standard rates", "SYSTEM")
        };

            await context.ClientGroupSet.AddRangeAsync(clientGroups);
            await context.SaveChangesAsync();
            Console.WriteLine("Seeded 3 client groups: VIP, Corporate, Retail");
        }

        var clientGroupsList = await context.ClientGroupSet.ToListAsync();

        // 2. Seed Exchange Rates
        if (!await context.ExchangeRateSet.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var effectiveFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

            var exchangeRates = new List<ExchangeRate>();

            // General Rates
            exchangeRates.Add(ExchangeRate.CreateGeneralRate(
                Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.02m, effectiveFrom, "SYSTEM", "CentralBank"));

            exchangeRates.Add(ExchangeRate.CreateGeneralRate(
                Currency.NGN, Currency.CNY, 0.0025m, 0.154m, 0.02m, effectiveFrom, "SYSTEM", "CentralBank"));

            // Group Rates
            if (clientGroupsList.Count >= 3)
            {
                exchangeRates.Add(ExchangeRate.CreateGroupRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.015m,
                    clientGroupsList[0].Id, effectiveFrom));

                exchangeRates.Add(ExchangeRate.CreateGroupRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.01m,
                    clientGroupsList[1].Id, effectiveFrom));

                exchangeRates.Add(ExchangeRate.CreateGroupRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.025m,
                    clientGroupsList[2].Id, effectiveFrom));
            }

            // Individual Client Rates
            var clients = await userManager.Users.Take(3).ToListAsync();
            if (clients.Count >= 3)
            {
                exchangeRates.Add(ExchangeRate.CreateIndividualRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.005m,
                    clients[0].Id, effectiveFrom));

                exchangeRates.Add(ExchangeRate.CreateIndividualRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.008m,
                    clients[1].Id, effectiveFrom));

                exchangeRates.Add(ExchangeRate.CreateIndividualRate(
                    Currency.XOF, Currency.CNY, 0.00167m, 0.154m, 0.012m,
                    clients[2].Id, effectiveFrom));
            }

            await context.ExchangeRateSet.AddRangeAsync(exchangeRates);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {exchangeRates.Count} exchange rates");
        }
    }
}