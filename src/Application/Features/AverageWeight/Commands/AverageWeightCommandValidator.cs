using Agrovet.Application.Features.AverageWeight.Dtos;
using FluentValidation;

namespace Agrovet.Application.Features.AverageWeight.Commands;

public abstract class AverageWeightBaseValidator<T> : ValidatorBase<T>
    where T : BaseAverageWeightRequest
{
    protected void AddCommonRules()
    {
        RuleFor(p => p.Estate)
            .NotEmpty()
            .WithMessage("Estate Code is required.")
            .NotNull()
            .WithMessage("Estate Code is required.")
            .MaximumLength(5)
            .WithMessage("Estate Code must not exceed 5 characters.");

        RuleFor(p => p.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .NotNull()
            .WithMessage("Status is required.")
            .MaximumLength(2)
            .WithMessage("Status must not exceed 2 characters.");
    }
}

public class CreateAverageWeightValidator : AverageWeightBaseValidator<CreateAverageWeightRequest>
{
    public CreateAverageWeightValidator()
    {
        AddCommonRules();
    }
}

public class CreateAverageWeightCommandValidator : AbstractValidator<CreateAverageWeightCommand>
{
    public CreateAverageWeightCommandValidator()
    {
        RuleFor(p => p.AverageWeight)
            .NotNull().WithMessage("Average weight cannot be empty.")
            .SetValidator(new CreateAverageWeightValidator()!);
    }
}

public class EditAverageWeightValidator : AverageWeightBaseValidator<EditAverageWeightRequest>
{
    public EditAverageWeightValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage("Code is required.")
            .NotNull()
            .WithMessage("Code is required.")
            .MaximumLength(5)
            .WithMessage("Code must not exceed 5 characters.");

        AddCommonRules();
    }
}

public class EditAverageWeightCommandValidator : AbstractValidator<EditAverageWeightCommand>
{
    public EditAverageWeightCommandValidator()
    {
        RuleFor(p => p.AverageWeight)
            .NotNull().WithMessage("AverageWeight cannot be empty.")
            .SetValidator(new EditAverageWeightValidator());
    }
}