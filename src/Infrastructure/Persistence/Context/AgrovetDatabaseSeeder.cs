using Agrovet.Application.Authorization;
using Agrovet.Domain.Entity.Auth;
using Agrovet.Domain.Entity.Inventory;
using Agrovet.Domain.Entity.Sales;
using Agrovet.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Dapper;

namespace Agrovet.Infrastructure.Persistence.Context;

public class AgrovetDatabaseSeeder(AgrovetContext context)
{
    public async Task SeedDatabaseAsync()
    {
        await CheckAndApplyPendingMigrationAsync();

        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();

        await SeedProductCategoriesAsync();
        await SeedProductsAsync();
        await SeedSuppliersAsync();
        await SeedOrderStatusesAsync();
        await SeedOrderTypesAsync();
        await SeedDistributionChannelsAsync();
        await SeedPriceItemsAsync();
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

    private async Task SeedProductCategoriesAsync()
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

        ProductCategory CreateItemCategory(string id, string name)
        {
            var itemCategory = ProductCategory.Create(name);

            itemCategory.SetId(id);

            itemCategory.SetPublicId(PublicId.CreateUnique().Value);

            return itemCategory;
        }
    }

    private async Task SeedDistributionChannelsAsync()
    {
        if (await context.DistributionChannelSet.AnyAsync())
            return;

        var seedData = new[]
        {
            new { Id = "01", Name = "Wholesale", Description = "Sales to resellers in bulk" },
            new { Id = "02", Name = "Retail", Description = "Direct sales to customers" },
            new { Id = "03", Name = "Online", Description = "E-commerce and online sales" }
        };

        var distributionChannels = seedData
            .Select(dc => CreateDistributionChannel(dc.Id, dc.Name, dc.Description))
            .ToList();

        await context.DistributionChannelSet.AddRangeAsync(distributionChannels);
        await context.SaveChangesAsync();
        return;

        DistributionChannel CreateDistributionChannel(string id, string name, string? description)
        {
            var channel = DistributionChannel.Create(name, description);

            channel.SetId(id);

            channel.SetPublicId(PublicId.CreateUnique().Value);

            return channel;
        }
    }

    private async Task SeedPriceItemsAsync()
    {
        if (await context.PriceItemSet.AnyAsync())
            return;

        var items = await context.ProductSet.Select(i => new { i.Id }).ToListAsync();
        var channels = await context.DistributionChannelSet.Select(c => new { c.Id }).ToListAsync();

        var seedData = new List<(string ChannelId, string ItemId, double Amount)>
        {
            // We'll generate prices systematically for demo purposes
        };

        foreach (var channel in channels)
        {
            foreach (var item in items)
            {
                const double basePrice = 150; // Example base price, can be adjusted

                var multiplier = channel.Id switch
                {
                    "01" => 0.9, // Wholesale gets 10% discount
                    "02" => 1.0, // Retail normal price
                    "03" => 1.1, // Online gets 10% premium
                    _ => 1.0
                };

                var price = basePrice * (double.Parse(item.Id) % 20); // Price variation by item
                price *= multiplier;

                seedData.Add((channel.Id, item.Id, Math.Round(price, 2)));
            }
        }

        var priceItems = seedData
            .Select(data => CreatePriceItem(data.ChannelId, data.ItemId, data.Amount))
            .ToList();

        await context.PriceItemSet.AddRangeAsync(priceItems);
        await context.SaveChangesAsync();

        return;

        PriceItem CreatePriceItem(string channelId, string itemId, double amount) =>
            PriceItem.Create(channelId, itemId, amount, DateTime.UtcNow);
    }

    private async Task SeedOrderStatusesAsync()
    {
        if (await context.OrderStatusSet.AnyAsync())
            return;

        var seedData = new[]
        {
            new { Id = "50", Name = "Pending Submission"},
            new { Id = "51", Name = "Submitted, Pending Validation"},
            new { Id = "52", Name = "Validated, Pending Reception"},
            new { Id = "53", Name = "Order Received"},
            new { Id = "54", Name = "Order Cancelled"},
            new { Id = "55", Name = "Order Closed"},
        };

        var itemCategories = seedData
            .Select(region => CreateOrderStatus(region.Id, region.Name)).ToList();

        await context.OrderStatusSet.AddRangeAsync(itemCategories);
        await context.SaveChangesAsync();
        return;

        OrderStatus CreateOrderStatus(string id, string name)
        {
            var orderStatus = OrderStatus.Create(name);

            orderStatus.SetId(id);

            orderStatus.SetPublicId(PublicId.CreateUnique().Value);

            return orderStatus;
        }
    }

    private async Task SeedOrderTypesAsync()
    {
        if (await context.OrderTypeSet.AnyAsync())
            return;

        var seedData = new[]
        {
            new { Id = "01", Name = "Bottling & Packaging"},
            new { Id = "02", Name = "External Order"},
        };

        var itemCategories = seedData
            .Select(region => CreateOrderType(region.Id, region.Name)).ToList();

        await context.OrderTypeSet.AddRangeAsync(itemCategories);
        await context.SaveChangesAsync();
        return;

        OrderType CreateOrderType(string id, string name)
        {
            var orderType = OrderType.Create(name);

            orderType.SetId(id);

            orderType.SetPublicId(PublicId.CreateUnique().Value);

            return orderType;
        }
    }

    private async Task SeedProductsAsync()
    {
        const string countSql = "SELECT COUNT(1) FROM inventory.product";
        const string insertSql = """
                                         INSERT INTO inventory.product
                                         (id, name, brand_name, bottling_type_size_in_liters, bottling_type_display_name,
                                          sku, category, status, min_stock, max_stock, reorder_lev, reorder_qtty, public_id, created_on)
                                         VALUES
                                         (@Id, @Name, @BrandName, @SizeInLiters, @DisplayName,
                                          @Sku, @Category, @Status, @MinStock, @MaxStock, @ReorderLev, @ReorderQtty, @PublicId, @CreatedOn);
                                 """;

        await using var conn = new NpgsqlConnection(context.Database.GetConnectionString());
        await conn.OpenAsync();

        var existingCount = await conn.ExecuteScalarAsync<int>(countSql);
        if (existingCount > 0)
            return;

        var brands = new[] { Brand.Engwari, Brand.Eposi, Brand.Lum };
        var bottlingTypes = new[]
        {
            BottlingType.FiveLiters, BottlingType.OneLiter, BottlingType.HalfLiter, BottlingType.ThreeLiters
        };

        var products = new List<dynamic>();
        var productId = 1;
        var createdOn = DateTime.UtcNow; // consistent timestamp

        foreach (var brand in brands)
        {
            foreach (var bottlingType in bottlingTypes)
            {
                var id = productId.ToString("D6"); // zero-padded

                // Construct the Product domain object
                var product = Product.Create(
                    brand,
                    bottlingType,
                    DefaultCategory,
                    DefaultStatus,
                    GetMinStockByPackaging(bottlingType),
                    GetMaxStockByPackaging(bottlingType),
                    GetReorderLevelByPackaging(bottlingType),
                    GetReorderQuantityByPackaging(bottlingType),
                    SkuGenerators.Deterministic, // this will be applied to the product
                    createdOn
                );

                product.SetId(id);
                product.SetPublicId(PublicId.CreateUnique().Value);

                // Now project to an anonymous object for insertion
                products.Add(new
                {
                    product.Id,
                    product.Name,
                    BrandName = product.Brand.Name,
                    product.BottlingType.SizeInLiters,
                    product.BottlingType.DisplayName,
                    product.Sku,
                    product.Category,
                    product.Status,
                    product.MinStock,
                    product.MaxStock,
                    product.ReorderLev,
                    product.ReorderQtty,
                    product.PublicId,
                    product.CreatedOn
                });

                productId++;
            }
        }

        try
        {
            await conn.ExecuteAsync(insertSql, products);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private const string DefaultCategory = "01";
    private const string DefaultStatus = "01";

    // Helper methods to determine stock levels based on packaging size
    private static double GetMinStockByPackaging(BottlingType bottlingType)
    {
        return bottlingType.SizeInLiters switch
        {
            5.0m => 10.0,   // 5L containers
            1.0m => 15.0,   // 1L containers  
            0.5m => 20.0,   // 1/2L containers
            3.0m => 8.0,    // 3L containers
            _ => 10.0
        };
    }

    private static double GetMaxStockByPackaging(BottlingType bottlingType)
    {
        return bottlingType.SizeInLiters switch
        {
            5.0m => 100.0,  // 5L containers
            1.0m => 150.0,  // 1L containers
            0.5m => 200.0,  // 1/2L containers  
            3.0m => 80.0,   // 3L containers
            _ => 100.0
        };
    }

    private static double GetReorderLevelByPackaging(BottlingType bottlingType)
    {
        return bottlingType.SizeInLiters switch
        {
            5.0m => 15.0,   // 5L containers
            1.0m => 25.0,   // 1L containers
            0.5m => 30.0,   // 1/2L containers
            3.0m => 12.0,   // 3L containers
            _ => 15.0
        };
    }

    private static double GetReorderQuantityByPackaging(BottlingType bottlingType)
    {
        return bottlingType.SizeInLiters switch
        {
            5.0m => 25.0,   // 5L containers
            1.0m => 40.0,   // 1L containers
            0.5m => 50.0,   // 1/2L containers
            3.0m => 20.0,   // 3L containers
            _ => 25.0
        };
    }

    private async Task SeedSuppliersAsync()
    {
        if (await context.SupplierSet.AnyAsync())
            return;

        var seedData = new[]
        {
            new
            {
                Id = "0001",
                Name = "AgroPalm Ltd.",
                Address = "123 Palm Avenue",
                City = "Douala",
                Phone = "+237671111111",
                ContactPerson = "John Doe",
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "0002",
                Name = "Eposi Distributors",
                Address = "45 Market Street",
                City = "Yaoundé",
                Phone = "+237672222222",
                ContactPerson = "Mary Jane",
                CreatedOn = DateTime.UtcNow
            },
            new
            {
                Id = "0003",
                Name = "Eposi Supplies",
                Address = "789 Trade Road",
                City = "Bamenda",
                Phone = "+237673333333",
                ContactPerson = "Peter Smith",
                CreatedOn = DateTime.UtcNow
            },
        };

        var suppliers = seedData
            .Select(s => CreateSupplier(
                s.Id,
                s.Name,
                s.Address,
                s.City,
                s.Phone,
                s.ContactPerson,
                s.CreatedOn))
            .ToList();

        await context.SupplierSet.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();
        return;

        Supplier CreateSupplier(
            string id,
            string name,
            string address,
            string city,
            string phone,
            string contactPerson,
            DateTime? createdOn)
        {
            var supplier = Supplier.Create(
                name,
                address,
                city,
                phone,
                contactPerson,
                createdOn);

            supplier.SetId(id);
            supplier.SetPublicId(PublicId.CreateUnique().Value);

            return supplier;
        }
    }

}