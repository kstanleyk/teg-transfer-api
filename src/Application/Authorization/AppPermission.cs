using System.Collections.ObjectModel;

namespace Agrovet.Application.Authorization
{
    public class AppPermission(string feature, string action, string group, string description, bool isBasic = false)
    {
        public string Feature { get; } = feature;
        public string Action { get; } = action;
        public string Group { get; } = group;
        public string Description { get; } = description;
        public bool IsBasic { get; } = isBasic;

        public string Name => NameFor(Feature, Action);

        public static string NameFor(string feature, string action)
        {
            return $"Permission.{feature}.{action}";
        }
    }

    public class AppPermissions
    {
        private static readonly AppPermission[] All = new AppPermission[]
        {
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

            // CreateDepartment
            new AppPermission(AppFeature.Department, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read CreateDepartment", true),
            new AppPermission(AppFeature.Department, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create CreateDepartment"),
            new AppPermission(AppFeature.Department, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update CreateDepartment"),

            // Enrollment
            new AppPermission(AppFeature.Enrollment, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Enrollment", true),
            new AppPermission(AppFeature.Enrollment, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Enrollment"),
            new AppPermission(AppFeature.Enrollment, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Enrollment"),

            new AppPermission(AppFeature.Country, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Country", true),
            new AppPermission(AppFeature.Country, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Country"),
            new AppPermission(AppFeature.Country, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Country"),

            new AppPermission(AppFeature.Employee, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Personal Info", true),

            new AppPermission(AppFeature.EmployeePersonal, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Personal Info", true),
            new AppPermission(AppFeature.EmployeePersonal, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Employee Personal Info"),

            new AppPermission(AppFeature.EmployeeIdentity, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Identity Info", true),
            new AppPermission(AppFeature.EmployeeIdentity, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Employee Identity Info"),

            new AppPermission(AppFeature.EmployeeContact, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Contact Info", true),
            new AppPermission(AppFeature.EmployeeContact, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Employee Contact Info"),

            new AppPermission(AppFeature.EmployeeNationality, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Nationality Info", true),
            new AppPermission(AppFeature.EmployeeNationality, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Employee Nationality Info"),

            new AppPermission(AppFeature.EmployeeEmployment, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Employee Employment Info", true),
            new AppPermission(AppFeature.EmployeeEmployment, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Employee Employment Info"),

            new AppPermission(AppFeature.EducationProfile, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Education Profile", true),
            new AppPermission(AppFeature.EducationProfile, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Education Profile"),
            new AppPermission(AppFeature.EducationProfile, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Education Profile"),

            new AppPermission(AppFeature.EducationLevel, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Education Level", true),
            new AppPermission(AppFeature.EducationLevel, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Education Level"),
            new AppPermission(AppFeature.EducationLevel, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Education Level"),

            new AppPermission(AppFeature.SalaryScale, AppAction.Read, AppRoleGroup.ManagementHierarchy, "Read Salary Scale", true),
            new AppPermission(AppFeature.SalaryScale, AppAction.Create, AppRoleGroup.ManagementHierarchy, "Create Salary Scale"),
            new AppPermission(AppFeature.SalaryScale, AppAction.Update, AppRoleGroup.ManagementHierarchy, "Update Salary Scale"),
        };

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

}