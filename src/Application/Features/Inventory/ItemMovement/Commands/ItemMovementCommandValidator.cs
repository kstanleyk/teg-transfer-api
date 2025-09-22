using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.ItemMovement.Commands;

public abstract class ItemMovementBaseValidator<T> : AbstractValidator<T>
    where T : BaseItemMovementRequest
{
    protected void AddCommonRules(ItemMovementValidationCodes validationCodes)
    {
        RuleFor(im => im.Id)
            .NotEmpty().WithMessage("Id is required.")
            .NotNull().WithMessage("Id is required.")
            .MaximumLength(50).WithMessage("Id must not exceed 50 characters.");

        RuleFor(im => im.LineNum)
            .NotEmpty().WithMessage("Line number is required.")
            .NotNull().WithMessage("Line number is required.")
            .MaximumLength(5).WithMessage("Line number must not exceed 5 characters.");

        RuleFor(im => im.Description)
            .NotEmpty().WithMessage("Description is required.")
            .NotNull().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(im => im.Item)
            .NotEmpty().WithMessage("Item is required.")
            .NotNull().WithMessage("Item is required.")
            .MaximumLength(100).WithMessage("Item must not exceed 100 characters.");

        RuleFor(im => im.TransDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .NotNull().WithMessage("Transaction date is required.");

        RuleFor(im => im.TransTime)
            .NotEmpty().WithMessage("Transaction time is required.")
            .NotNull().WithMessage("Transaction time is required.")
            .MaximumLength(10).WithMessage("Transaction time must not exceed 10 characters.");

        RuleFor(im => im.Sense)
            .NotEmpty().WithMessage("Sense is required.")
            .NotNull().WithMessage("Sense is required.")
            .MaximumLength(10).WithMessage("Sense must not exceed 10 characters.")
            .Must(s => s == "IN" || s == "OUT")
            .WithMessage("Sense must be either 'IN' or 'OUT'.");

        RuleFor(im => im.Qtty)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(im => im.SourceId)
            .NotEmpty().WithMessage("SourceId is required.")
            .NotNull().WithMessage("SourceId is required.")
            .MaximumLength(100).WithMessage("SourceId must not exceed 100 characters.");

        RuleFor(im => im.SourceLineNum)
            .NotEmpty().WithMessage("Source line number is required.")
            .NotNull().WithMessage("Source line number is required.")
            .MaximumLength(50).WithMessage("Source line number must not exceed 50 characters.");
    }
}

public class CreateItemMovementValidator : ItemMovementBaseValidator<CreateItemMovementRequest>
{
    public CreateItemMovementValidator(ItemMovementValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditItemMovementValidator : ItemMovementBaseValidator<EditItemMovementRequest>
{
    public EditItemMovementValidator(ItemMovementValidationCodes validationCodes)
    {
        RuleFor(im => im.Id)
            .NotEmpty().WithMessage("Id is required for edit.")
            .NotNull().WithMessage("Id is required for edit.")
            .MaximumLength(50).WithMessage("Id must not exceed 50 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateItemMovementCommandValidator : AbstractValidator<CreateItemMovementCommand>
{
    public CreateItemMovementCommandValidator(ItemMovementValidationCodes validationCodes)
    {
        RuleFor(p => p.ItemMovement)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new CreateItemMovementValidator(validationCodes));
    }
}

public class EditItemMovementCommandValidator : AbstractValidator<EditItemMovementCommand>
{
    public EditItemMovementCommandValidator(ItemMovementValidationCodes validationCodes)
    {
        RuleFor(p => p.ItemMovement)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new EditItemMovementValidator(validationCodes));
    }
}