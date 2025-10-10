using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Country.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Country.Commands;

public class CreateCountryCommandResponse : BaseResponse
{
    public CountryCreatedResponse Data { get; set; } = null!;
}

public class CreateCountryCommand : IRequest<CreateCountryCommandResponse>
{
    public required CreateCountryRequest Country { get; set; }
}

public class CreateCountryCommandHandler(ICountryRepository countryRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateCountryCommand, CreateCountryCommandResponse>
{
    public async Task<CreateCountryCommandResponse> Handle(CreateCountryCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateCountryCommandResponse();

        var validator = new CreateCountryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Country == null)
            throw new ArgumentNullException(nameof(request.Country));

        var icr = request.Country;

        var country = Transfer.Domain.Entity.Inventory.Country.Create(icr.Name);

        country.SetPublicId(PublicId.CreateUnique().Value);

        var result = await countryRepository.AddAsync(country);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<CountryCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        countryRepository.Dispose();
    }
}