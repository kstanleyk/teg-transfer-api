using Microsoft.EntityFrameworkCore;
using Transfer.Application.Authorization;
using Transfer.Domain.Entity.Auth;
using Transfer.Domain.Entity.Core;
using Transfer.Domain.ValueObjects;

namespace Transfer.Infrastructure.Persistence.Context;

public class AgrovetDatabaseSeeder(AgrovetContext context)
{
    public async Task SeedDatabaseAsync()
    {
        await CheckAndApplyPendingMigrationAsync();

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();

        await SeedWarehousesAsync();
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

    private async Task SeedWarehousesAsync()
    {
        if (await context.WarehouseSet.AnyAsync())
            return;

        var warehouses = new List<Warehouse>();

        // Cameroon Production Warehouse
        var cameroonAddress = Address.CreateCameroonAddress(
            city: "Douala",
            quarter: "Bonaberi",
            landmark: "Near Douala Port",
            region: "LT");

        var cameroonWarehouse = Warehouse.Create("Douala Production Facility", cameroonAddress);
        cameroonWarehouse.SetId("CM001");
        cameroonWarehouse.SetPublicId(PublicId.CreateUnique().Value);
        warehouses.Add(cameroonWarehouse);

        // US Warehouses
        var usWarehouses = new[]
        {
            new { Id = "US001", Name = "Miramar FL Distribution Center", Street = "5360 SW 150th Terrace", City = "Miramar", State = "FL", Zip = "33026" },
            new { Id = "US002", Name = "Laurel MD Distribution Center", Street = "9157 Whiskey Bottom Road", City = "LAUREL", State = "MD", Zip = "20723" },
            new { Id = "US003", Name = "Burtonsville MD Distribution Center", Street = "4311 Ambrose Court", City = "Burtonsville", State = "MD", Zip = "20866" },
            new { Id = "US004", Name = "Chicago IL Distribution Center", Street = "1133 Keystone Avenue", City = "Chicago", State = "IL", Zip = "60651" },
            new { Id = "US005", Name = "Overland Park KS Distribution Center", Street = "10205 West 147 St", City = "Overland Park", State = "KS", Zip = "66221" }
        };

        foreach (var wh in usWarehouses)
        {
            var usAddress = Address.CreateUsAddress(street: wh.Street, city: wh.City, state: wh.State, zipCode: wh.Zip);

            var warehouse = Warehouse.Create(wh.Name, usAddress);
            warehouse.SetId(wh.Id);
            warehouse.SetPublicId(PublicId.CreateUnique().Value);
            warehouses.Add(warehouse);
        }

        await context.WarehouseSet.AddRangeAsync(warehouses);
        await context.SaveChangesAsync();
    }
}