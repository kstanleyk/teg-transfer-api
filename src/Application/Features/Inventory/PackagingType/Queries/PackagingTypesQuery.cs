using MediatR;
using Transfer.Application.Features.Inventory.BottlingType.Queries;
using Transfer.Application.Features.Inventory.PackagingType.Dtos;

namespace Transfer.Application.Features.Inventory.PackagingType.Queries;

public record PackagingTypesQuery : IRequest<PackagingTypeResponse[]>;

public class PackagingTypesQueryHandler : RequestHandlerBase, IRequestHandler<PackagingTypesQuery, PackagingTypeResponse[]>
{
    public Task<PackagingTypeResponse[]> Handle(PackagingTypesQuery request, CancellationToken cancellationToken)
    {
        var packagingTypes = Transfer.Domain.Entity.Inventory.PackagingType.All
            .Select(PackagingTypeMapper.ToDto)
            .ToArray();

        return Task.FromResult(packagingTypes);
    }
}

public static class PackagingTypeMapper
{
    public static PackagingTypeResponse ToDto(this Transfer.Domain.Entity.Inventory.PackagingType packagingType)
    {
        var bottlingType = BottlingTypeMapper.ToDto(packagingType.BottlingType);

        var displayName = packagingType.UnitsPerBox > 1
            ? $"{packagingType.DisplayName}"
            : $"{bottlingType.DisplayName} / Single Unit";

        return new PackagingTypeResponse(
            packagingType.Id,
            bottlingType,
            packagingType.UnitsPerBox,
            displayName
        );
    }
}
