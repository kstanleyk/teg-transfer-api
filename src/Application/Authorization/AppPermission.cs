using System.Collections.ObjectModel;

namespace Agrovet.Application.Authorization;

public class AppPermission(string feature, string action, string group, string description, bool isBasic = false)
{
    public string Feature { get; } = feature;
    public string Action { get; } = action;
    public string Group { get; } = group;
    public string Description { get; } = description;
    public bool IsBasic { get; } = isBasic;

    public string Name => NameFor(Feature, Action);

    public static string NameFor(string feature, string action) => $"Permission.{feature}.{action}";
}

public class AppPermissions
{
    private static readonly AppPermission[] All =
    [
        new AppPermission(AppFeature.Users, AppAction.Create, AppRoleGroup.SystemAccess, "Create Users"),
        new AppPermission(AppFeature.Users, AppAction.Update, AppRoleGroup.SystemAccess, "Update Users"),
        new AppPermission(AppFeature.Users, AppAction.Read, AppRoleGroup.SystemAccess, "Read Users"),
        new AppPermission(AppFeature.Users, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Users"),

        new AppPermission(AppFeature.UserRoles, AppAction.Read, AppRoleGroup.SystemAccess, "Read User Roles"),
        new AppPermission(AppFeature.UserRoles, AppAction.Update, AppRoleGroup.SystemAccess, "Update User Roles"),

        new AppPermission(AppFeature.Roles, AppAction.Read, AppRoleGroup.SystemAccess, "Read Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Create, AppRoleGroup.SystemAccess, "Create Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Update, AppRoleGroup.SystemAccess, "Update Roles"),
        new AppPermission(AppFeature.Roles, AppAction.Delete, AppRoleGroup.SystemAccess, "Delete Roles"),

        new AppPermission(AppFeature.RoleClaims, AppAction.Read, AppRoleGroup.SystemAccess, "Read Role Claims/Permissions"),
        new AppPermission(AppFeature.RoleClaims, AppAction.Update, AppRoleGroup.SystemAccess, "Update Role Claims/Permissions"),

        new AppPermission(AppFeature.Item, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Inventory Product", true),
        new AppPermission(AppFeature.Item, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Inventory Product"),
        new AppPermission(AppFeature.Item, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Inventory Product"),

        new AppPermission(AppFeature.ItemCategory, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read item category", true),
        new AppPermission(AppFeature.ItemCategory, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create item category"),
        new AppPermission(AppFeature.ItemCategory, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update item category"),

        new AppPermission(AppFeature.ItemMovement, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read item category", true),
        new AppPermission(AppFeature.ItemMovement, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create item category"),
        new AppPermission(AppFeature.ItemMovement, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update item category"),

        new AppPermission(AppFeature.Order, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read orders", true),
        new AppPermission(AppFeature.Order, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create orders"),
        new AppPermission(AppFeature.Order, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update orders"),
        new AppPermission(AppFeature.Order, AppAction.Submit, AppRoleGroup.ManagementHierarchy, "Submit orders"),
        new AppPermission(AppFeature.Order, AppAction.Validate, AppRoleGroup.ManagementHierarchy, "Validate orders"),
        new AppPermission(AppFeature.Order, AppAction.Receive, AppRoleGroup.ManagementHierarchy, "Receive orders"),

        new AppPermission(AppFeature.OrderDetail, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read order details", true),
        new AppPermission(AppFeature.OrderDetail, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create order details"),
        new AppPermission(AppFeature.OrderDetail, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update order details"),

        new AppPermission(AppFeature.Supplier, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read supplier", true),
        new AppPermission(AppFeature.Supplier, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create supplier"),
        new AppPermission(AppFeature.Supplier, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update supplier"),

        new AppPermission(AppFeature.OrderType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read order type", true),
        new AppPermission(AppFeature.OrderType, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create order type"),
        new AppPermission(AppFeature.OrderType, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update order type"),

        new AppPermission(AppFeature.OrderStatus, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read order status", true),
        new AppPermission(AppFeature.OrderStatus, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create order status"),
        new AppPermission(AppFeature.OrderStatus, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update order status"),

        new AppPermission(AppFeature.DistributionChannel, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read distribution channels", true),
        new AppPermission(AppFeature.DistributionChannel, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create distribution channels"),
        new AppPermission(AppFeature.DistributionChannel, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update distribution channels"),

        new AppPermission(AppFeature.BottlingType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read bottling types", true),
        new AppPermission(AppFeature.PackagingType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read packaging types", true),

        new AppPermission(AppFeature.PriceItem, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read price items", true),
        new AppPermission(AppFeature.PriceItem, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create price items"),
        new AppPermission(AppFeature.PriceItem, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update price items"),
    ];

    public static IReadOnlyList<AppPermission> AdminPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All.Where(p => !p.IsBasic).ToArray());

    public static IReadOnlyList<AppPermission> BasicPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All.Where(p => p.IsBasic).ToArray());

    public static IReadOnlyList<AppPermission> AllPermissions { get; } =
        new ReadOnlyCollection<AppPermission>(All);

    public static IReadOnlyList<FeatureWithAction> AppFeatures { get; } =
        AppPermissions.AllPermissions
            .GroupBy(p => p.Feature)
            .Select(g => new FeatureWithAction
            {
                Feature = g.Key,
                Actions = g.Select(p => new ActionDetail
                {
                    Action = p.Action,
                    Description = p.Description,
                    IsBasic = p.IsBasic
                }).ToList()
            })
            .ToList();

}

public class ActionDetail
{
    public required string Action { get; set; }
    public required string Description { get; set; }
    public bool IsBasic { get; set; }
}

public class FeatureWithAction
{
    public required string Feature { get; set; }
    public required List<ActionDetail> Actions { get; set; }
}