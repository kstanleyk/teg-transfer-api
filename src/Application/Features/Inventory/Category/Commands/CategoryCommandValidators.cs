using FluentValidation;
using Transfer.Application.Features.Inventory.Category.Dtos;

namespace Transfer.Application.Features.Inventory.Category.Commands;

public abstract class CategoryBaseValidator<T> : AbstractValidator<T>
    where T : BaseCategoryRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateCategoryValidator : CategoryBaseValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        AddCommonRules();
    }
}

public class EditCategoryValidator : CategoryBaseValidator<EditCategoryRequest>
{
    public EditCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(p => p.Category)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateCategoryValidator());
    }
}

public class EditCategoryCommandValidator : AbstractValidator<EditCategoryCommand>
{
    public EditCategoryCommandValidator()
    {
        RuleFor(p => p.Category)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditCategoryValidator());
    }
}