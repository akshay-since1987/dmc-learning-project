using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Proposals.Commands;
using ProposalManagement.Application.Proposals.Queries;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/proposals")]
[Authorize]
public class ProposalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProposalsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get proposal stats for dashboard</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProposalStatsQuery(), ct);
        return Ok(result);
    }

    /// <summary>Get proposals submitted by the current user</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyProposalsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Get all proposals (Commissioner, Auditor, Lotus)</summary>
    [HttpGet]
    [Authorize(Roles = "Commissioner,Auditor,Lotus")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? stage, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllProposalsQuery(search, stage, pageIndex, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Get proposals pending at the current user's stage</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "CityEngineer,ADO,ChiefAccountant,DeputyCommissioner,Commissioner")]
    public async Task<IActionResult> GetPending([FromQuery] string? search, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPendingApprovalsQuery(search, pageIndex, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Get proposal by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProposalByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Create a new proposal (draft)</summary>
    [HttpPost]
    [Authorize(Roles = "Submitter,Lotus")]
    public async Task<IActionResult> Create([FromBody] CreateProposalCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data }) : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Update a draft/pushed-back proposal</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProposalCommand command, CancellationToken ct = default)
    {
        if (id != command.Id) return BadRequest(new { message = "ID mismatch" });
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Delete (soft) a draft proposal</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteProposalCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Submit a draft proposal for approval</summary>
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] LegacySubmitProposalRequest body, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SubmitProposalCommand(
            id,
            body.SignaturePageNumber,
            body.SignaturePositionX,
            body.SignaturePositionY,
            body.SignatureWidth,
            body.SignatureHeight,
            body.SignatureRotation,
            body.GeneratedPdfPath), ct);
        return result.IsSuccess
            ? Ok(new { message = "Proposal submitted successfully", historyId = result.Data!.HistoryId, newStage = result.Data.NewStage })
            : StatusCode(result.StatusCode, new { result.Error });
    }
}

public record LegacySubmitProposalRequest(
    int SignaturePageNumber,
    decimal SignaturePositionX,
    decimal SignaturePositionY,
    decimal SignatureWidth,
    decimal SignatureHeight,
    decimal SignatureRotation,
    string GeneratedPdfPath);
