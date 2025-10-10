using AutoMapper;
using Transfer.Application.Features.Sales.PriceItem.Dtos;

namespace Transfer.Application.Features.Sales.PriceItem;

public class PriceItemProfile : Profile
{
    public PriceItemProfile()
    {
        CreateMap<Transfer.Domain.Entity.Sales.PriceItem, PriceItemResponse>();

        CreateMap<Transfer.Domain.Entity.Sales.PriceItem, PriceItemUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Sales.PriceItem, PriceItemCreatedResponse>();
    }
}