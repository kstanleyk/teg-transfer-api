using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Departments.Queries;
using Agrovet.Application.Models.Core.Department;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers;

public class DepartmentsController(IMediator mediator) : ApiControllerBase<DepartmentsController>
{
    [HttpGet]
    [MustHavePermission(AppFeature.AverageWeight, AppAction.Read)]
    public async Task<ActionResult<IEnumerable<DepartmentRequest>>> GetDepartments()
    {
        var departments = await mediator.Send(new GetDepartmentsQuery());
        return Ok(departments);
    }
}