using Agrovet.Application.Features.AverageWeight.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.AverageWeight.Commands;

public class EditAverageWeightCommandResponse : BaseResponse
{
    public AverageWeightUpdatedResponse Data { get; set; } = null!;
}

public class EditAverageWeightCommand : IRequest<EditAverageWeightCommandResponse>
{
    public required EditAverageWeightRequest AverageWeight { get; set; }
}

public class EditAverageWeightCommandHandler(
    IAverageWeightRepository averageWeightRepository,
    IEstateRepository estateRepository,
    IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditAverageWeightCommand, EditAverageWeightCommandResponse>
{
    public async Task<EditAverageWeightCommandResponse> Handle(EditAverageWeightCommand request, CancellationToken cancellationToken)
    {
        var response = new EditAverageWeightCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new AverageWeightValidationCodes
        {
            EstateCodes = ids
        };

        var validator = new EditAverageWeightCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var awr = request.AverageWeight;

        var averageWeight = Domain.Entity.Core.AverageWeight.Create(awr.Id, awr.Estate, awr.Block, awr.Weight, awr.EffectiveDate,
            awr.Status, DateTime.UtcNow);

        averageWeight.SetId(awr.Id);
        averageWeight.SetPublicId(awr.PublicId);

        var result = await averageWeightRepository.EditAsync(averageWeight);

        if (result.Status != RepositoryActionStatus.Updated && result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<AverageWeightUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
        estateRepository.Dispose();
    }
}