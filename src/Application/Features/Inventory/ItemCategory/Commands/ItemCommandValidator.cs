using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Commands;

public abstract class ItemCategoryBaseValidator<T> : AbstractValidator<T>
    where T : BaseItemCategoryRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateItemCategoryValidator : ItemCategoryBaseValidator<CreateItemCategoryRequest>
{
    public CreateItemCategoryValidator()
    {
        AddCommonRules();
    }
}

public class EditItemCategoryValidator : ItemCategoryBaseValidator<EditItemCategoryRequest>
{
    public EditItemCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
{
    public CreateItemCategoryCommandValidator()
    {
        RuleFor(p => p.ItemCategory)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new CreateItemCategoryValidator());
    }
}

public class EditItemCategoryCommandValidator : AbstractValidator<EditItemCategoryCommand>
{
    public EditItemCategoryCommandValidator()
    {
        RuleFor(p => p.ItemCategory)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new EditItemCategoryValidator());
    }
}