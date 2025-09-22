namespace Agrovet.Application.Authorization;

public static class AppFeature
{
    //IdentityInfo
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string RoleClaims = nameof(RoleClaims);

    //Inventory
    public const string Item = nameof(Item);
    public const string ItemCategory = nameof(ItemCategory);
    public const string ItemMovement = nameof(ItemMovement);
    public const string Order = nameof(Order);
    public const string OrderDetail = nameof(OrderDetail);
}
