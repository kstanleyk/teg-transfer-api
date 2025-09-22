using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Core;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;
using SequentialGuid;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Commands;

public class CreateItemCategoryCommandResponse : BaseResponse
{
    public ItemCategoryCreatedResponse Data { get; set; } = null!;
}

public class CreateItemCategoryCommand : IRequest<CreateItemCategoryCommandResponse>
{
    public required CreateItemCategoryRequest ItemCategory { get; set; }
}

public class CreateItemCategoryCommandHandler(IItemCategoryRepository itemCategoryRepository,
    IEstateRepository estateRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateItemCategoryCommand, CreateItemCategoryCommandResponse>
{
    public async Task<CreateItemCategoryCommandResponse> Handle(CreateItemCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateItemCategoryCommandResponse();

        var ids = await estateRepository.GetIdsAsync();

        var validationCodes = new ItemCategoryValidationCodes
        {
            ValidIds = ids
        };

        var validator = new CreateItemCategoryCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.ItemCategory == null)
            throw new ArgumentNullException(nameof(request.ItemCategory));

        var icr = request.ItemCategory;

        var itemCategory = Domain.Entity.Inventory.ItemCategory.Create(icr.Name, DateTime.UtcNow);

        itemCategory.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await itemCategoryRepository.AddAsync(itemCategory);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ItemCategoryCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
        estateRepository.Dispose();
    }
}