using Agrovet.Application.Features.Sales.PriceItem.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Sales.PriceItem;

public class PriceItemProfile : Profile
{
    public PriceItemProfile()
    {
        CreateMap<Domain.Entity.Sales.PriceItem, PriceItemResponse>();

        CreateMap<Domain.Entity.Sales.PriceItem, PriceItemUpdatedResponse>();

        CreateMap<Domain.Entity.Sales.PriceItem, PriceItemCreatedResponse>();
    }
}