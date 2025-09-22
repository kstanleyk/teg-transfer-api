using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;
using SequentialGuid;

namespace Agrovet.Application.Features.Inventory.ItemMovement.Commands;

public class CreateItemMovementCommandResponse : BaseResponse
{
    public ItemMovementCreatedResponse Data { get; set; } = null!;
}

public class CreateItemMovementCommand : IRequest<CreateItemMovementCommandResponse>
{
    public required CreateItemMovementRequest ItemMovement { get; set; }
}

public class CreateItemMovementCommandHandler(IItemMovementRepository averageWeightRepository,
    IEstateRepository estateRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateItemMovementCommand, CreateItemMovementCommandResponse>
{
    public async Task<CreateItemMovementCommandResponse> Handle(CreateItemMovementCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateItemMovementCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new ItemMovementValidationCodes
        {
            ValidSenses = ids
        };

        var validator = new CreateItemMovementCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.ItemMovement == null)
            throw new ArgumentNullException(nameof(request.ItemMovement));

        var imr = request.ItemMovement;

        var itemMovement = Domain.Entity.Inventory.ItemMovement.Create(imr.LineNum, imr.Description, imr.Item,
            imr.TransDate, imr.TransTime, imr.Sense, imr.Qtty, imr.SourceId, imr.SourceLineNum, DateTime.UtcNow);

        itemMovement.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await averageWeightRepository.AddAsync(itemMovement);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemMovementCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
        estateRepository.Dispose();
    }
}