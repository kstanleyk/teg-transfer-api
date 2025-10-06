using Agrovet.Application.Features.Sales.DistributionChannel.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Sales.DistributionChannel;

public class DistributionChannelProfile : Profile
{
    public DistributionChannelProfile()
    {
        CreateMap<Domain.Entity.Sales.DistributionChannel, DistributionChannelResponse>();

        CreateMap<Domain.Entity.Sales.DistributionChannel, DistributionChannelUpdatedResponse>();

        CreateMap<Domain.Entity.Sales.DistributionChannel, DistributionChannelCreatedResponse>();
    }
}