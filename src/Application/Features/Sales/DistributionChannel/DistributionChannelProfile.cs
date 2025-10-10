using AutoMapper;
using Transfer.Application.Features.Sales.DistributionChannel.Dtos;

namespace Transfer.Application.Features.Sales.DistributionChannel;

public class DistributionChannelProfile : Profile
{
    public DistributionChannelProfile()
    {
        CreateMap<Transfer.Domain.Entity.Sales.DistributionChannel, DistributionChannelResponse>();

        CreateMap<Transfer.Domain.Entity.Sales.DistributionChannel, DistributionChannelUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Sales.DistributionChannel, DistributionChannelCreatedResponse>();
    }
}