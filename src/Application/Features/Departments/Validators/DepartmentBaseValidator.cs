using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BaseDepartmentDto = Agrovet.Application.Models.Core.Department.BaseDepartmentDto;

namespace Agrovet.Application.Features.Departments.Validators
{
    public abstract class DepartmentBaseValidator<T> : AbstractValidator<T>
        where T : BaseDepartmentDto
    {
        protected void AddCommonRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters");

            RuleFor(x => x.FacultyId)
                .NotEmpty().WithMessage("FacultyId is required")
                .MaximumLength(3).WithMessage("FacultyId must not exceed 3 characters");
        }
    }
}
