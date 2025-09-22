using FluentValidation;

namespace Agrovet.Application.Features.Inventory.Order.Commands;

public class OrderDetailRequest
{
    public string OrderId { get; set; } = null!;
    public string LineNum { get; set; } = null!;
    public string Item { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Qtty { get; set; }
    public double UnitCost { get; set; }
    public double Amount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime TransDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
}

public abstract class OrderDetailBaseValidator<T> : AbstractValidator<T>
    where T : OrderDetailRequest
{
    protected void AddCommonRules()
    {
        RuleFor(d => d.OrderId)
            .NotEmpty().WithMessage("OrderId is required.")
            .NotNull().WithMessage("OrderId is required.")
            .MaximumLength(15).WithMessage("OrderId must not exceed 15 characters.");

        RuleFor(d => d.LineNum)
            .NotEmpty().WithMessage("Line number is required.")
            .NotNull().WithMessage("Line number is required.")
            .MaximumLength(5).WithMessage("Line number must not exceed 5 characters.");

        RuleFor(d => d.Item)
            .NotEmpty().WithMessage("Item is required.")
            .NotNull().WithMessage("Item is required.")
            .MaximumLength(10).WithMessage("Item must not exceed 10 characters.");

        RuleFor(d => d.Description)
            .NotEmpty().WithMessage("Description is required.")
            .NotNull().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(d => d.Qtty)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(d => d.UnitCost)
            .GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");

        RuleFor(d => d.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

        RuleFor(d => d.Status)
            .NotEmpty().WithMessage("Status is required.")
            .NotNull().WithMessage("Status is required.")
            .MaximumLength(5).WithMessage("Status must not exceed 5 characters.");

        RuleFor(d => d.TransDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .NotNull().WithMessage("Transaction date is required.");

        // Optional: validate ExpiryDate and ReceiveDate
        RuleFor(d => d.ExpiryDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(100))
            .When(d => d.ExpiryDate.HasValue)
            .WithMessage("ExpiryDate is invalid.");

        RuleFor(d => d.ReceiveDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(d => d.ReceiveDate.HasValue)
            .WithMessage("ReceiveDate cannot be in the future.");
    }
}

public class CreateOrderDetailValidator : OrderDetailBaseValidator<OrderDetailRequest>
{
    public CreateOrderDetailValidator()
    {
        AddCommonRules();
    }
}

public class EditOrderDetailValidator : OrderDetailBaseValidator<OrderDetailRequest>
{
    public EditOrderDetailValidator()
    {
        RuleFor(d => d.OrderId)
            .NotEmpty().WithMessage("OrderId is required for edit.")
            .NotNull().WithMessage("OrderId is required for edit.")
            .MaximumLength(15).WithMessage("OrderId must not exceed 15 characters.");

        RuleFor(d => d.LineNum)
            .NotEmpty().WithMessage("Line number is required for edit.")
            .NotNull().WithMessage("Line number is required for edit.")
            .MaximumLength(5).WithMessage("Line number must not exceed 5 characters.");

        AddCommonRules();
    }
}