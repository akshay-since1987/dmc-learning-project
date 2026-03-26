using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Masters.Commands;
using ProposalManagement.Application.Masters.Queries;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MastersController : ControllerBase
{
    private readonly IMediator _mediator;
    public MastersController(IMediator mediator) => _mediator = mediator;

    // --- Departments ---
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDepartmentsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("departments/{id:guid}")]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    // --- Designations ---
    [HttpGet("designations")]
    public async Task<IActionResult> GetDesignations([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDesignationsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("designations/{id:guid}")]
    public async Task<IActionResult> GetDesignationById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetDesignationByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    // --- Fund Types ---
    [HttpGet("fund-types")]
    public async Task<IActionResult> GetFundTypes([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFundTypesQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("fund-types/{id:guid}")]
    public async Task<IActionResult> GetFundTypeById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFundTypeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    // --- Account Heads ---
    [HttpGet("account-heads")]
    public async Task<IActionResult> GetAccountHeads([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAccountHeadsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("account-heads/{id:guid}")]
    public async Task<IActionResult> GetAccountHeadById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAccountHeadByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    // --- Wards ---
    [HttpGet("wards")]
    public async Task<IActionResult> GetWards([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWardsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("wards/{id:guid}")]
    public async Task<IActionResult> GetWardById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWardByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    // --- Procurement Methods ---
    [HttpGet("procurement-methods")]
    public async Task<IActionResult> GetProcurementMethods([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProcurementMethodsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("procurement-methods/{id:guid}")]
    public async Task<IActionResult> GetProcurementMethodById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProcurementMethodByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }
}
