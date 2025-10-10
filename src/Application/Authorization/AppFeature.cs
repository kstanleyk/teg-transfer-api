namespace Transfer.Application.Authorization;

public static class AppFeature
{
    //IdentityInfo
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string RoleClaims = nameof(RoleClaims);

    //Inventory
    public const string Item = nameof(Item);
    public const string Category = nameof(Category);
    public const string ItemMovement = nameof(ItemMovement);
    public const string Order = nameof(Order);
    public const string OrderDetail = nameof(OrderDetail);
    public const string Supplier = nameof(Supplier);
    public const string OrderType = nameof(OrderType);
    public const string OrderStatus = nameof(OrderStatus);
    public const string DistributionChannel = nameof(DistributionChannel);
    public const string PriceItem = nameof(PriceItem);
    public const string BottlingType = nameof(BottlingType);
    public const string PackagingType = nameof(PackagingType);
}
