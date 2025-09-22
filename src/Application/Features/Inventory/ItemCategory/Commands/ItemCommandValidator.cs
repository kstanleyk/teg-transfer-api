using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Commands;

public abstract class ItemCategoryBaseValidator<T> : AbstractValidator<T>
    where T : BaseItemCategoryRequest
{
    protected void AddCommonRules(ItemCategoryValidationCodes validationCodes)
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category Name is required.")
            .NotNull().WithMessage("Category Name is required.")
            .MaximumLength(150).WithMessage("Category Name must not exceed 150 characters.");
    }
}

public class CreateItemCategoryValidator : ItemCategoryBaseValidator<CreateItemCategoryRequest>
{
    public CreateItemCategoryValidator(ItemCategoryValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditItemCategoryValidator : ItemCategoryBaseValidator<EditItemCategoryRequest>
{
    public EditItemCategoryValidator(ItemCategoryValidationCodes validationCodes)
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category Code is required for edit.")
            .NotNull().WithMessage("Category Code is required for edit.")
            .MaximumLength(5).WithMessage("Category Code must not exceed 5 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
{
    public CreateItemCategoryCommandValidator(ItemCategoryValidationCodes validationCodes)
    {
        RuleFor(p => p.ItemCategory)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new CreateItemCategoryValidator(validationCodes));
    }
}

public class EditItemCategoryCommandValidator : AbstractValidator<EditItemCategoryCommand>
{
    public EditItemCategoryCommandValidator(ItemCategoryValidationCodes validationCodes)
    {
        RuleFor(p => p.ItemCategory)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new EditItemCategoryValidator(validationCodes));
    }
}