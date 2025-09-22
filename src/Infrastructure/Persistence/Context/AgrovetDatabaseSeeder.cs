using Agrovet.Application.Authorization;
using Agrovet.Domain.Entity.Auth;
using Agrovet.Domain.Entity.Inventory;
using Agrovet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Agrovet.Infrastructure.Persistence.Context;

public class AgrovetDatabaseSeeder(AgrovetContext context)
{
    public async Task SeedDatabaseAsync()
    {
        await CheckAndApplyPendingMigrationAsync();

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();

        await SeedItemCategoriesAsync();
        await SeedItemsAsync();
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

    private async Task SeedItemCategoriesAsync()
    {
        if (await context.ItemCategorySet.AnyAsync())
            return;

        var seedData = new[]
        {
            new { Id = "01", Name = "Palm Oil"},
            new { Id = "02", Name = "Egusi"},
        };

        var itemCategories = seedData
            .Select(region => CreateItemCategory(region.Id, region.Name)).ToList();

        await context.ItemCategorySet.AddRangeAsync(itemCategories);
        await context.SaveChangesAsync();
        return;

        ItemCategory CreateItemCategory(string id, string name)
        {
            var itemCategory = ItemCategory.Create(name);

            itemCategory.SetId(id);

            itemCategory.SetPublicId(PublicId.CreateUnique().Value);

            return itemCategory;
        }
    }

    private async Task SeedItemsAsync()
    {
        if (await context.ItemSet.AnyAsync())
            return;

        var seedData = new[]
        {
            new
            {
                Id = "000001",
                Name = "Palm Oil, Lum 5l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "LM00001",
                Brand = "Lum",
                Category = "01",
                Status = "01",
                MinStock = 10.0,
                MaxStock = 100.0,
                ReorderLev = 15.0,
                ReorderQtty = 25.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000002",
                Name = "Palm Oil, Engwari 5l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EN00002",
                Brand = "Engwari",
                Category = "01",
                Status = "01",
                MinStock = 5.0,
                MaxStock = 50.0,
                ReorderLev = 10.0,
                ReorderQtty = 15.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000003",
                Name = "Palm Oil, Eposi 5l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EP00003",
                Brand = "Eposi",
                Category = "01",
                Status = "01",
                MinStock = 8.0,
                MaxStock = 80.0,
                ReorderLev = 12.0,
                ReorderQtty = 20.0,
                CreatedOn = DateTime.UtcNow
            },

            new
            {
                Id = "000004",
                Name = "Palm Oil, Lum 1l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "LM00004",
                Brand = "Lum",
                Category = "01",
                Status = "01",
                MinStock = 10.0,
                MaxStock = 100.0,
                ReorderLev = 15.0,
                ReorderQtty = 25.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000005",
                Name = "Palm Oil, Engwari 1l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EN00005",
                Brand = "Engwari",
                Category = "01",
                Status = "01",
                MinStock = 5.0,
                MaxStock = 50.0,
                ReorderLev = 10.0,
                ReorderQtty = 15.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000006",
                Name = "Palm Oil, Eposi 1l",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EP00006",
                Brand = "Eposi",
                Category = "01",
                Status = "01",
                MinStock = 8.0,
                MaxStock = 80.0,
                ReorderLev = 12.0,
                ReorderQtty = 20.0,
                CreatedOn = DateTime.UtcNow
            },

            new
            {
                Id = "000007",
                Name = "Palm Oil, Lum 33cl",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "LM00007",
                Brand = "Lum",
                Category = "01",
                Status = "01",
                MinStock = 10.0,
                MaxStock = 100.0,
                ReorderLev = 15.0,
                ReorderQtty = 25.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000008",
                Name = "Palm Oil, Engwari 33cl",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EN00008",
                Brand = "Engwari",
                Category = "01",
                Status = "01",
                MinStock = 5.0,
                MaxStock = 50.0,
                ReorderLev = 10.0,
                ReorderQtty = 15.0,
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "000009",
                Name = "Palm Oil, Eposi 33cl",
                ShortDescription = "Cameroon Palm Oil",
                BarCodeText = "EP00009",
                Brand = "Eposi",
                Category = "01",
                Status = "01",
                MinStock = 8.0,
                MaxStock = 80.0,
                ReorderLev = 12.0,
                ReorderQtty = 20.0,
                CreatedOn = DateTime.UtcNow
            },
        };

        var items = seedData
            .Select(item => CreateItem(item.Id, item.Name, item.ShortDescription, item.BarCodeText, item.Brand,
                item.Category, item.Status, item.MinStock, item.MaxStock, item.ReorderLev, item.ReorderQtty,
                item.CreatedOn)).ToList();

        await context.ItemSet.AddRangeAsync(items);
        await context.SaveChangesAsync();
        return;

        Item CreateItem(string id, string name, string shortDescription, string barCodeText, string brand, string category,
            string status, double minStock, double maxStock, double reorderLev, double reorderQtty,
            DateTime? createdOn)
        {
            var item = Item.Create(name, shortDescription, barCodeText, brand, category,
                status, minStock, maxStock, reorderLev, reorderQtty, createdOn);

            item.SetId(id);

            item.SetPublicId(PublicId.CreateUnique().Value);

            return item;
        }
    }
}