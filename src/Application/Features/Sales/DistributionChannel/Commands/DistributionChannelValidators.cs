using FluentValidation;
using Transfer.Application.Features.Sales.DistributionChannel.Dtos;

namespace Transfer.Application.Features.Sales.DistributionChannel.Commands;

public abstract class DistributionChannelBaseValidator<T> : AbstractValidator<T>
    where T : BaseDistributionChannelRequest
{
    protected void AddCommonRules()
    {

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .NotNull().WithMessage("Category name is required.")
            .MaximumLength(50).WithMessage("Category name must not exceed 50 characters.");
    }
}

public class CreateDistributionChannelValidator : DistributionChannelBaseValidator<CreateDistributionChannelRequest>
{
    public CreateDistributionChannelValidator()
    {
        AddCommonRules();
    }
}

public class EditDistributionChannelValidator : DistributionChannelBaseValidator<EditDistributionChannelRequest>
{
    public EditDistributionChannelValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Category code is required for edit.")
            .NotNull().WithMessage("Category code is required for edit.")
            .MaximumLength(2).WithMessage("Category code must not exceed 2 characters.");

        AddCommonRules();
    }
}

public class CreateDistributionChannelCommandValidator : AbstractValidator<CreateDistributionChannelCommand>
{
    public CreateDistributionChannelCommandValidator()
    {
        RuleFor(p => p.DistributionChannel)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new CreateDistributionChannelValidator());
    }
}

public class EditDistributionChannelCommandValidator : AbstractValidator<EditDistributionChannelCommand>
{
    public EditDistributionChannelCommandValidator()
    {
        RuleFor(p => p.DistributionChannel)
            .NotNull().WithMessage("Product category cannot be empty.")
            .SetValidator(new EditDistributionChannelValidator());
    }
}