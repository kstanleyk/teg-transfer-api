using Agrovet.Application.Features.Core.AverageWeight.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using AutoMapper;
using MediatR;
using SequentialGuid;

namespace Agrovet.Application.Features.Core.AverageWeight.Commands;

public class CreateAverageWeightCommandResponse : BaseResponse
{
    public AverageWeightCreatedResponse Data { get; set; } = null!;
}

public class CreateAverageWeightCommand : IRequest<CreateAverageWeightCommandResponse>
{
    public required CreateAverageWeightRequest AverageWeight { get; set; }
}

public class CreateAverageWeightCommandHandler(
    IAverageWeightRepository averageWeightRepository,
    IEstateRepository estateRepository,
    IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateAverageWeightCommand, CreateAverageWeightCommandResponse>
{
    public async Task<CreateAverageWeightCommandResponse> Handle(CreateAverageWeightCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateAverageWeightCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new AverageWeightValidationCodes
        {
            EstateCodes = ids
        };

        var validator = new CreateAverageWeightCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.AverageWeight == null)
            throw new ArgumentNullException(nameof(request.AverageWeight));

        var awr = request.AverageWeight;

        var averageWeight = Domain.Entity.Core.AverageWeight.Create(awr.Estate, awr.Block, awr.Weight, awr.EffectiveDate,
            awr.Status, DateTime.UtcNow);

        averageWeight.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await averageWeightRepository.AddAsync(averageWeight);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<AverageWeightCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
        estateRepository.Dispose();
    }
}