using Agrovet.Application.Features.Sales.DistributionChannel.Dtos;
using Agrovet.Application.Interfaces.Sales;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Sales.DistributionChannel.Queries;

public record DistributionChannelsQuery : IRequest<DistributionChannelResponse[]>;

public class DistributionChannelsQueryHandler(IDistributionChannelRepository distributionChannelRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<DistributionChannelsQuery, DistributionChannelResponse[]>
{

    public async Task<DistributionChannelResponse[]> Handle(DistributionChannelsQuery request, CancellationToken cancellationToken)
    {
        var itemCategories = await distributionChannelRepository.GetAllAsync();
        return mapper.Map<DistributionChannelResponse[]>(itemCategories);
    }

    protected override void DisposeCore()
    {
        distributionChannelRepository.Dispose();
    }
}