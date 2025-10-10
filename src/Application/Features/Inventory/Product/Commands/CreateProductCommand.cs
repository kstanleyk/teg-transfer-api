using AutoMapper;
using MediatR;
using SequentialGuid;
using Transfer.Application.Features.Inventory.Product.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Inventory;
using Transfer.Domain.Entity.Inventory;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Inventory.Product.Commands;

public class CreateProductCommandResponse : BaseResponse
{
    public ProductCreatedResponse Data { get; set; } = null!;
}

public class CreateProductCommand : IRequest<CreateProductCommandResponse>
{
    public required CreateProductRequest Product { get; set; }
}

public class CreateProductCommandHandler(IProductRepository productRepository,ICategoryRepository categoryRepository, IMapper mapper) : 
    RequestHandlerBase, IRequestHandler<CreateProductCommand, CreateProductCommandResponse>
{
    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateProductCommandResponse();

        if (request.Product == null)
            throw new ArgumentNullException(nameof(request.Product));

        var categoryIds = await categoryRepository.GetAllIdsAsync();
        var validationCodes = new ItemValidationCodes
        {
            CategoryCodes = categoryIds
        };

        // Validate the request
        var validator = new CreateItemCommandValidator(validationCodes);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var productRequest = request.Product;

        var brand = Brand.FromName(productRequest.Brand);
        var packagingType = Transfer.Domain.Entity.Inventory.BottlingType.FiveLiters;

        var product = Transfer.Domain.Entity.Inventory.Product.Create(brand, packagingType, productRequest.Category, productRequest.Status,
            productRequest.MinStock, productRequest.MaxStock, productRequest.ReorderLev,
            productRequest.ReorderQtty, SkuGenerators.Deterministic, DateTime.UtcNow);

        product.SetPublicId(PublicId.CreateUnique().Value);

        product.SetPublicId(SequentialGuidGenerator.Instance.NewGuid());

        var result = await productRepository.AddAsync(product);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<ProductCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        productRepository.Dispose();
        categoryRepository.Dispose();
    }
}
