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

        new AppPermission(AppFeature.AverageWeight, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read CreateDepartment", true),
        new AppPermission(AppFeature.AverageWeight, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create CreateDepartment"),
        new AppPermission(AppFeature.AverageWeight, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update CreateDepartment"),

        new AppPermission(AppFeature.Block, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read block", true),
        new AppPermission(AppFeature.Block, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create block"),
        new AppPermission(AppFeature.Block, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update block"),

        new AppPermission(AppFeature.Estate, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read estate", true),
        new AppPermission(AppFeature.Estate, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create estate"),
        new AppPermission(AppFeature.Estate, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update estate"),

        new AppPermission(AppFeature.EstateTask, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read estate task", true),
        new AppPermission(AppFeature.EstateTask, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create estate task"),
        new AppPermission(AppFeature.EstateTask, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update estate task"),

        new AppPermission(AppFeature.EstateTaskType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read estate task type", true),
        new AppPermission(AppFeature.EstateTaskType, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create estate task type"),
        new AppPermission(AppFeature.EstateTaskType, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update estate task type"),

        new AppPermission(AppFeature.ExpenseSource, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read expense source", true),
        new AppPermission(AppFeature.ExpenseSource, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create expense source"),
        new AppPermission(AppFeature.ExpenseSource, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update expense source"),

        new AppPermission(AppFeature.ExpenseStatus, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read expense status", true),
        new AppPermission(AppFeature.ExpenseStatus, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create expense status"),
        new AppPermission(AppFeature.ExpenseStatus, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update expense status"),

        new AppPermission(AppFeature.ExpenseType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read expense type", true),
        new AppPermission(AppFeature.ExpenseType, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create expense type"),
        new AppPermission(AppFeature.ExpenseType, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update expense type"),

        new AppPermission(AppFeature.ExpenseTypeInventory, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read expense type inventory", true),
        new AppPermission(AppFeature.ExpenseTypeInventory, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create expense type inventory"),
        new AppPermission(AppFeature.ExpenseTypeInventory, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update expense type inventory"),

        new AppPermission(AppFeature.Operation, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read payroll farm operations", true),
        new AppPermission(AppFeature.Operation, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create payroll farm operations"),
        new AppPermission(AppFeature.Operation, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update payroll farm operations"),

        new AppPermission(AppFeature.Payroll, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read payroll", true),
        new AppPermission(AppFeature.Payroll, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create payroll"),
        new AppPermission(AppFeature.Payroll, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update payroll"),

        new AppPermission(AppFeature.PayrollAverageWeight, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read payroll average weight", true),
        new AppPermission(AppFeature.PayrollAverageWeight, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create payroll average weight"),
        new AppPermission(AppFeature.PayrollAverageWeight, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update payroll average weight"),

        new AppPermission(AppFeature.Plant, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read plants", true),
        new AppPermission(AppFeature.Plant, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create plants"),
        new AppPermission(AppFeature.Plant, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update plants"),

        new AppPermission(AppFeature.Task, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read tasks", true),
        new AppPermission(AppFeature.Task, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create tasks"),
        new AppPermission(AppFeature.Task, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update tasks"),

        new AppPermission(AppFeature.TaskType, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read task type", true),
        new AppPermission(AppFeature.TaskType, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create task type"),
        new AppPermission(AppFeature.TaskType, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update task type"),

        new AppPermission(AppFeature.TaskTypeAccount, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read task type account", true),
        new AppPermission(AppFeature.TaskTypeAccount, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create task type account"),
        new AppPermission(AppFeature.TaskTypeAccount, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update task type account"),

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