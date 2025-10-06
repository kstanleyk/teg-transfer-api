using MediatR;
using Agrovet.Application.Features.Inventory.BottlingType.Dtos;

namespace Agrovet.Application.Features.Inventory.BottlingType.Queries;

public record BottlingTypesQuery : IRequest<BottlingTypeResponse[]>;

public class BottlingTypesQueryHandler : RequestHandlerBase, IRequestHandler<BottlingTypesQuery, BottlingTypeResponse[]>
{
    public Task<BottlingTypeResponse[]> Handle(BottlingTypesQuery request, CancellationToken cancellationToken)
    {
        var bottlingTypes = Domain.Entity.Inventory.BottlingType.All
            .Select(BottlingTypeMapper.ToDto)
            .ToArray();

        return Task.FromResult(bottlingTypes);
    }
}

public static class BottlingTypeMapper
{
    public static BottlingTypeResponse ToDto(Domain.Entity.Inventory.BottlingType bottlingType) =>
        bottlingType switch
        {
            _ when ReferenceEquals(bottlingType, Domain.Entity.Inventory.BottlingType.OneLiter) => new BottlingTypeResponse("OneLiter", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Domain.Entity.Inventory.BottlingType.HalfLiter) => new BottlingTypeResponse("HalfLiter", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Domain.Entity.Inventory.BottlingType.ThreeLiters) => new BottlingTypeResponse("ThreeLiters", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Domain.Entity.Inventory.BottlingType.FiveLiters) => new BottlingTypeResponse("FiveLiters", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ => throw new ArgumentOutOfRangeException(nameof(bottlingType), "Unknown bottling type")
        };
}