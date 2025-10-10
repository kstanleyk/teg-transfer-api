using MediatR;
using Transfer.Application.Features.Inventory.BottlingType.Dtos;

namespace Transfer.Application.Features.Inventory.BottlingType.Queries;

public record BottlingTypesQuery : IRequest<BottlingTypeResponse[]>;

public class BottlingTypesQueryHandler : RequestHandlerBase, IRequestHandler<BottlingTypesQuery, BottlingTypeResponse[]>
{
    public Task<BottlingTypeResponse[]> Handle(BottlingTypesQuery request, CancellationToken cancellationToken)
    {
        var bottlingTypes = Transfer.Domain.Entity.Inventory.BottlingType.All
            .Select(BottlingTypeMapper.ToDto)
            .ToArray();

        return Task.FromResult(bottlingTypes);
    }
}

public static class BottlingTypeMapper
{
    public static BottlingTypeResponse ToDto(Transfer.Domain.Entity.Inventory.BottlingType bottlingType) =>
        bottlingType switch
        {
            _ when ReferenceEquals(bottlingType, Transfer.Domain.Entity.Inventory.BottlingType.OneLiter) => new BottlingTypeResponse("OneLiter", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Transfer.Domain.Entity.Inventory.BottlingType.HalfLiter) => new BottlingTypeResponse("HalfLiter", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Transfer.Domain.Entity.Inventory.BottlingType.ThreeLiters) => new BottlingTypeResponse("ThreeLiters", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ when ReferenceEquals(bottlingType, Transfer.Domain.Entity.Inventory.BottlingType.FiveLiters) => new BottlingTypeResponse("FiveLiters", bottlingType.SizeInLiters, bottlingType.DisplayName),
            _ => throw new ArgumentOutOfRangeException(nameof(bottlingType), "Unknown bottling type")
        };
}