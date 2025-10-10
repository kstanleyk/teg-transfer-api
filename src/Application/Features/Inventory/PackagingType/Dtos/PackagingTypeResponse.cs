using Transfer.Application.Features.Inventory.BottlingType.Dtos;

namespace Transfer.Application.Features.Inventory.PackagingType.Dtos;

public record PackagingTypeResponse(string Id, BottlingTypeResponse BottlingType, int UnitsPerBox, string DisplayName);