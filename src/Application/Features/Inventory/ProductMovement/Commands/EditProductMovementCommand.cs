using Agrovet.Application.Features.Inventory.ProductMovement.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductMovement.Commands;

public class EditProductMovementCommandResponse : BaseResponse
{
    public ProductMovementUpdatedResponse Data { get; set; } = null!;
}

public class EditProductMovementCommand : IRequest<EditProductMovementCommandResponse>
{
    public required EditProductMovementRequest ProductMovement { get; set; }
}

public class EditProductMovementCommandHandler(
    IProductMovementRepository averageWeightRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<EditProductMovementCommand, EditProductMovementCommandResponse>
{
    public async Task<EditProductMovementCommandResponse> Handle(EditProductMovementCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditProductMovementCommandResponse();

        var validationCodes = new ProductMovementValidationCodes
        {
            ValidSenses = []
        };

        var validator = new EditProductMovementCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var imr = request.ProductMovement;

        var itemMovement = Domain.Entity.Inventory.ProductMovement.Create(imr.LineNum, imr.Description, imr.Item,
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

        response.Data = mapper.Map<ProductMovementUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
    }
}