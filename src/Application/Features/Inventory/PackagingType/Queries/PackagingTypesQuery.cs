using Agrovet.Application.Features.Inventory.BottlingType.Queries;
using Agrovet.Application.Features.Inventory.PackagingType.Dtos;
using MediatR;

namespace Agrovet.Application.Features.Inventory.PackagingType.Queries;

public record PackagingTypesQuery : IRequest<PackagingTypeResponse[]>;

public class PackagingTypesQueryHandler : RequestHandlerBase, IRequestHandler<PackagingTypesQuery, PackagingTypeResponse[]>
{
    public Task<PackagingTypeResponse[]> Handle(PackagingTypesQuery request, CancellationToken cancellationToken)
    {
        var packagingTypes = Domain.Entity.Inventory.PackagingType.All
            .Select(PackagingTypeMapper.ToDto)
            .ToArray();

        return Task.FromResult(packagingTypes);
    }
}

public static class PackagingTypeMapper
{
    public static PackagingTypeResponse ToDto(this Domain.Entity.Inventory.PackagingType packagingType) =>
        packagingType switch
        {
            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.OneLiterCarton)
                => CreateResponse("OneLiterCarton", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.HalfLiterCarton)
                => CreateResponse("HalfLiterCarton", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.ThreeLitersCarton)
                => CreateResponse("ThreeLitersCarton", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.FiveLitersCarton)
                => CreateResponse("FiveLitersCarton", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.OneLiterUnit)
                => CreateResponse("OneLiterUnit", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.HalfLiterUnit)
                => CreateResponse("HalfLiterUnit", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.ThreeLitersUnit)
                => CreateResponse("ThreeLitersUnit", packagingType),

            _ when ReferenceEquals(packagingType, Domain.Entity.Inventory.PackagingType.FiveLitersUnit)
                => CreateResponse("FiveLitersUnit", packagingType),

            _ => throw new ArgumentOutOfRangeException(nameof(packagingType), "Unknown packaging type")
        };

    private static PackagingTypeResponse CreateResponse(string id, Domain.Entity.Inventory.PackagingType packagingType)
    {
        var bottlingType = BottlingTypeMapper.ToDto(packagingType.BottlingType);

        var displayName = packagingType.UnitsPerBox > 1
            ? $"{bottlingType.DisplayName} / {packagingType.DisplayName}"
            : bottlingType.DisplayName;

        return new PackagingTypeResponse(id, bottlingType, packagingType.UnitsPerBox, displayName);
    }
}