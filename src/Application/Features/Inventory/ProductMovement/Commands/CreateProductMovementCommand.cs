using AutoMapper;
using MediatR;
using SequentialGuid;
using Transfer.Application.Features.Inventory.ProductMovement.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;

namespace Transfer.Application.Features.Inventory.ProductMovement.Commands;

public class CreateProductMovementCommandResponse : BaseResponse
{
    public ProductMovementCreatedResponse Data { get; set; } = null!;
}

public class CreateProductMovementCommand : IRequest<CreateProductMovementCommandResponse>
{
    public required CreateProductMovementRequest ProductMovement { get; set; }
}

public class CreateProductMovementCommandHandler(IProductMovementRepository averageWeightRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateProductMovementCommand, CreateProductMovementCommandResponse>
{
    public async Task<CreateProductMovementCommandResponse> Handle(CreateProductMovementCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateProductMovementCommandResponse();

        var validationCodes = new ProductMovementValidationCodes
        {
            ValidSenses = []
        };

        var validator = new CreateProductMovementCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.ProductMovement == null)
            throw new ArgumentNullException(nameof(request.ProductMovement));

        var imr = request.ProductMovement;

        var itemMovement = Transfer.Domain.Entity.Inventory.ProductMovement.Create(imr.LineNum, imr.Description, imr.Item,
            imr.TransDate, imr.TransTime, imr.Sense, imr.Qtty, imr.SourceId, imr.SourceLineNum, DateTime.UtcNow);

        itemMovement.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await averageWeightRepository.AddAsync(itemMovement);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ProductMovementCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        averageWeightRepository.Dispose();
    }
}