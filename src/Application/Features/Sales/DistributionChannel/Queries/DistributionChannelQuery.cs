using Agrovet.Application.Features.Sales.DistributionChannel.Dtos;
using Agrovet.Application.Interfaces.Sales;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Sales.DistributionChannel.Queries;

public record DistributionChannelQuery : IRequest<DistributionChannelResponse>
{
    public required Guid PublicId { get; set; }
}

public class DistributionChannelQueryHandler(IDistributionChannelRepository distributionChannelRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<DistributionChannelQuery, DistributionChannelResponse>
{

    public async Task<DistributionChannelResponse> Handle(DistributionChannelQuery request, CancellationToken cancellationToken)
    {
        var distributionChannel = await distributionChannelRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<DistributionChannelResponse>(distributionChannel);
    }

    protected override void DisposeCore()
    {
        distributionChannelRepository.Dispose();
    }
}