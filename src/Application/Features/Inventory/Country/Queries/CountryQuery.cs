using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Country.Dtos;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Country.Queries;

public record CountryQuery : IRequest<CountryResponse>
{
    public required Guid PublicId { get; set; }
}

public class CountryQueryHandler(ICountryRepository countryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<CountryQuery, CountryResponse>
{

    public async Task<CountryResponse> Handle(CountryQuery request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<CountryResponse>(country);
    }

    protected override void DisposeCore()
    {
        countryRepository.Dispose();
    }
}