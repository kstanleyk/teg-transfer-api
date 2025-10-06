using Agrovet.Application.Features.Inventory.ProductCategory.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.ProductCategory.Commands;

public abstract class ProductCategoryBaseValidator<T> : AbstractValidator<T>
    where T : BaseProductCategoryRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateProductCategoryValidator : ProductCategoryBaseValidator<CreateProductCategoryRequest>
{
    public CreateProductCategoryValidator()
    {
        AddCommonRules();
    }
}

public class EditProductCategoryValidator : ProductCategoryBaseValidator<EditProductCategoryRequest>
{
    public EditProductCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(p => p.ProductCategory)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateProductCategoryValidator());
    }
}

public class EditProductCategoryCommandValidator : AbstractValidator<EditProductCategoryCommand>
{
    public EditProductCategoryCommandValidator()
    {
        RuleFor(p => p.ProductCategory)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditProductCategoryValidator());
    }
}