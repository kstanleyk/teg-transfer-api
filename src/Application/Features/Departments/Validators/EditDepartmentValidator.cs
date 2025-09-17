using Agrovet.Application.Features.Departments.Commands;
using Agrovet.Application.Models.Core.Department;
using FluentValidation;

namespace Agrovet.Application.Features.Departments.Validators;

public class EditDepartmentValidator : DepartmentBaseValidator<EditDepartmentRequest>
{
    public EditDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required")
            .MaximumLength(3).WithMessage("Id must not exceed 3 characters");

        AddCommonRules();
    }
}

public class EditDepartmentCommandValidator : AbstractValidator<EditDepartmentCommand>
{
    public EditDepartmentCommandValidator()
    {
        RuleFor(p => p.Department)
            .NotNull().WithMessage("AverageWeight cannot be empty.")
            .SetValidator(new EditDepartmentValidator());
    }
}