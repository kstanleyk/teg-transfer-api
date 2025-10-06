using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using Agrovet.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductCategory.Commands;

public class CreateProductCategoryCommandResponse : BaseResponse
{
    public ProductCategoryCreatedResponse Data { get; set; } = null!;
}

public class CreateProductCategoryCommand : IRequest<CreateProductCategoryCommandResponse>
{
    public required CreateProductCategoryRequest ProductCategory { get; set; }
}

public class CreateProductCategoryCommandHandler(IProductCategoryRepository productCategoryRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateProductCategoryCommand, CreateProductCategoryCommandResponse>
{
    public async Task<CreateProductCategoryCommandResponse> Handle(CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateProductCategoryCommandResponse();

        var validator = new CreateProductCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.ProductCategory == null)
            throw new ArgumentNullException(nameof(request.ProductCategory));

        var icr = request.ProductCategory;

        var itemCategory = Domain.Entity.Inventory.ProductCategory.Create(icr.Name);

        itemCategory.SetPublicId(PublicId.CreateUnique().Value);

        var result = await productCategoryRepository.AddAsync(itemCategory);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ProductCategoryCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        productCategoryRepository.Dispose();
    }
}