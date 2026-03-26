using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Masters.Commands;
using ProposalManagement.Application.Masters.Queries;

namespace ProposalManagement.Api.Controllers.Lotus;

[ApiController]
[Route("api/lotus/fund-types")]
[Authorize(Roles = "Lotus")]
public class LotusFundTypesController : ControllerBase
{
    private readonly IMediator _mediator;
    public LotusFundTypesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFundTypesQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetFundTypeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFundTypeCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data }) : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFundTypeCommand command, CancellationToken ct = default)
    {
        if (id != command.Id) return BadRequest(new { message = "ID mismatch" });
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteFundTypeCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }
}
