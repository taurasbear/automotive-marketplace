using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Domain.Constants;

public static class DefaultUserPermissions
{
    public static readonly IReadOnlyList<Permission> All =
    [
        Permission.ViewListings,
        Permission.CreateListings,
        Permission.ManageListings,
        Permission.ViewModels,
        Permission.ViewVariants,
        Permission.ViewMakes,
    ];
}
