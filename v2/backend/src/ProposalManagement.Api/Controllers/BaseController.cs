using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult ToActionResult(Result result) =>
        result.IsSuccess ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });

    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(new { success = true, data = result.Data }) : StatusCode(result.StatusCode, new { success = false, error = result.Error });
}
