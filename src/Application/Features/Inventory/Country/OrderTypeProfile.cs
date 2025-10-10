using AutoMapper;
using Transfer.Application.Features.Inventory.Country.Dtos;

namespace Transfer.Application.Features.Inventory.Country;

public class CountryProfile : Profile
{
    public CountryProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Country, CountryResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Country, CountryUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.Country, CountryCreatedResponse>();
    }
}