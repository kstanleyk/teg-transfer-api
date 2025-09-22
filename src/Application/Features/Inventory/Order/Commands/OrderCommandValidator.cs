using Agrovet.Application.Features.Inventory.Order.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public abstract class OrderBaseValidator<T> : AbstractValidator<T>
    where T : BaseOrderRequest
{
    protected void AddCommonRules(OrderValidationCodes validationCodes)
    {
        RuleFor(o => o.OrderType)
            .NotEmpty().WithMessage("Order Type is required.")
            .NotNull().WithMessage("Order Type is required.")
            .MaximumLength(5).WithMessage("Order Type must not exceed 5 characters.");

        RuleFor(o => o.Status)
            .NotEmpty().WithMessage("Status is required.")
            .NotNull().WithMessage("Status is required.")
            .MaximumLength(5).WithMessage("Status must not exceed 5 characters.");

        RuleFor(o => o.Description)
            .NotEmpty().WithMessage("Description is required.")
            .NotNull().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(o => o.Supplier)
            .NotEmpty().WithMessage("Supplier is required.")
            .NotNull().WithMessage("Supplier is required.")
            .MaximumLength(15).WithMessage("Supplier must not exceed 15 characters.");

        RuleFor(o => o.TransDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .NotNull().WithMessage("Transaction date is required.");

        RuleFor(o => o.OrderDate)
            .NotEmpty().WithMessage("Order date is required.")
            .NotNull().WithMessage("Order date is required.");
    }
}

public class CreateOrderValidator : OrderBaseValidator<CreateOrderRequest>
{
    public CreateOrderValidator(OrderValidationCodes validationCodes)
    {
        AddCommonRules(validationCodes);
    }
}

public class EditOrderValidator : OrderBaseValidator<EditOrderRequest>
{
    public EditOrderValidator(OrderValidationCodes validationCodes)
    {
        RuleFor(o => o.Id)
            .NotEmpty().WithMessage("Order Id is required.")
            .NotNull().WithMessage("Order Id is required.")
            .MaximumLength(15).WithMessage("Order Id must not exceed 15 characters.");

        AddCommonRules(validationCodes);
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator(OrderValidationCodes validationCodes)
    {
        RuleFor(p => p.Order)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new CreateOrderValidator(validationCodes));
    }
}

public class EditOrderCommandValidator : AbstractValidator<EditOrderCommand>
{
    public EditOrderCommandValidator(OrderValidationCodes validationCodes)
    {
        RuleFor(p => p.Order)
            .NotNull().WithMessage("Item category cannot be empty.")
            .SetValidator(new EditOrderValidator(validationCodes));
    }
}