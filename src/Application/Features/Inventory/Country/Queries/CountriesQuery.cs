using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Country.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Country.Queries;

public record CountriesQuery : IRequest<CountryResponse[]>;

public class CountriesQueryHandler(ICountryRepository countryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<CountriesQuery, CountryResponse[]>
{

    public async Task<CountryResponse[]> Handle(CountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await countryRepository.GetAllAsync();
        return mapper.Map<CountryResponse[]>(countries);
    }

    protected override void DisposeCore()
    {
        countryRepository.Dispose();
    }
}