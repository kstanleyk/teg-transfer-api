using Agrovet.Application.Features.Inventory.BottlingType.Dtos;

namespace Agrovet.Application.Features.Inventory.PackagingType.Dtos;

public record PackagingTypeResponse(string Id, BottlingTypeResponse BottlingType, int UnitsPerBox, string DisplayName);