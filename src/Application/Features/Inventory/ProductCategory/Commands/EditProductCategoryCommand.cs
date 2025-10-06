using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ProductCategory.Commands;

public class EditProductCategoryCommandResponse : BaseResponse
{
    public ProductCategoryUpdatedResponse Data { get; set; } = null!;
}

public class EditProductCategoryCommand : IRequest<EditProductCategoryCommandResponse>
{
    public required EditProductCategoryRequest ProductCategory { get; set; }
}

public class EditProductCategoryCommandHandler(IProductCategoryRepository productCategoryRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditProductCategoryCommand, EditProductCategoryCommandResponse>
{
    public async Task<EditProductCategoryCommandResponse> Handle(EditProductCategoryCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditProductCategoryCommandResponse();

        var validator = new EditProductCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.ProductCategory;

        var itemCategory = Domain.Entity.Inventory.ProductCategory.Create(icr.Name);
        itemCategory.SetId(icr.Id);
        itemCategory.SetPublicId(icr.PublicId);

        var result = await productCategoryRepository.UpdateAsyncAsync(icr.PublicId, itemCategory);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ProductCategoryUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        productCategoryRepository.Dispose();
    }
}