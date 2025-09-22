using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemMovement.Commands;

public class EditItemMovementCommandResponse : BaseResponse
{
    public ItemMovementUpdatedResponse Data { get; set; } = null!;
}

public class EditItemMovementCommand : IRequest<EditItemMovementCommandResponse>
{
    public required EditItemMovementRequest ItemMovement { get; set; }
}

public class EditItemMovementCommandHandler(
    IItemMovementRepository averageWeightRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditItemMovementCommand, EditItemMovementCommandResponse>
{
    public async Task<EditItemMovementCommandResponse> Handle(EditItemMovementCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditItemMovementCommandResponse();

        var validationCodes = new ItemMovementValidationCodes
        {
            ValidSenses = []
        };

        var validator = new EditItemMovementCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var imr = request.ItemMovement;

        var itemMovement = Domain.Entity.Inventory.ItemMovement.Create(imr.LineNum, imr.Description, imr.Item,
            imr.TransDate, imr.TransTime, imr.Sense, imr.Qtty, imr.SourceId, imr.SourceLineNum, DateTime.UtcNow);

        itemMovement.SetId(imr.Id);
        itemMovement.SetPublicId(imr.PublicId);

        var result = await averageWeightRepository.EditAsync(itemMovement);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemMovementUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
    }
}