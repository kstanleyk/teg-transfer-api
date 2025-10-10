using AutoMapper;
using MediatR;
using Transfer.Application.Features.Inventory.Category.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Category.Commands;

public class CreateCategoryCommandResponse : BaseResponse
{
    public CategoryCreatedResponse Data { get; set; } = null!;
}

public class CreateCategoryCommand : IRequest<CreateCategoryCommandResponse>
{
    public required CreateCategoryRequest Category { get; set; }
}

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateCategoryCommand, CreateCategoryCommandResponse>
{
    public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateCategoryCommandResponse();

        var validator = new CreateCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.Category == null)
            throw new ArgumentNullException(nameof(request.Category));

        var icr = request.Category;

        var itemCategory = Transfer.Domain.Entity.Inventory.Category.Create(icr.Name);

        itemCategory.SetPublicId(PublicId.CreateUnique().Value);

        var result = await categoryRepository.AddAsync(itemCategory);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<CategoryCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        categoryRepository.Dispose();
    }
}