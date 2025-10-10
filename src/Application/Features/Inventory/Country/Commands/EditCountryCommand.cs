using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Country.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.Country.Commands;

public class EditCountryCommandResponse : BaseResponse
{
    public CountryUpdatedResponse Data { get; set; } = null!;
}

public class EditCountryCommand : IRequest<EditCountryCommandResponse>
{
    public required EditCountryRequest Country { get; set; }
}

public class EditCountryCommandHandler(ICountryRepository countryRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditCountryCommand, EditCountryCommandResponse>
{
    public async Task<EditCountryCommandResponse> Handle(EditCountryCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditCountryCommandResponse();

        var validator = new EditCountryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.Country;

        var country = Transfer.Domain.Entity.Inventory.Country.Create(icr.Name);
        country.SetId(icr.Id);
        country.SetPublicId(icr.PublicId);

        var result = await countryRepository.UpdateAsyncAsync(icr.PublicId, country);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<CountryUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        countryRepository.Dispose();
    }
}