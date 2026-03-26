using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.V1.Dashboard;
using ProposalManagement.Application.V1.Proposals.Commands;
using ProposalManagement.Application.V1.Proposals.Queries;
using ProposalManagement.Application.Lotus.Queries;

namespace ProposalManagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/proposals")]
[Authorize]
public class ProposalsV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    public ProposalsV1Controller(IMediator mediator) => _mediator = mediator;

    /// <summary>Get proposal wizard data by ID (all steps)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProposalWizardQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Save a specific wizard step. Step 1 with no ProposalId creates a new proposal.</summary>
    [HttpPost("step")]
    [Authorize(Roles = "Submitter,CityEngineer,ADO,ChiefAccountant,Lotus")]
    public async Task<IActionResult> SaveStep([FromBody] SaveProposalStepCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Data })
            : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Submit a completed proposal for approval workflow</summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Submitter,Lotus")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitProposalRequest body, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new Application.Proposals.Commands.SubmitProposalCommand(
            id,
            body.SignaturePageNumber,
            body.SignaturePositionX,
            body.SignaturePositionY,
            body.SignatureWidth,
            body.SignatureHeight,
            body.SignatureRotation,
            body.GeneratedPdfPath), ct);
        return result.IsSuccess
            ? Ok(new { message = "Proposal submitted for approval", historyId = result.Data!.HistoryId, newStage = result.Data.NewStage })
            : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Get list of accounting officers (ChiefAccountant + ADO users) for the wizard step 3 dropdown</summary>
    [HttpGet("accounting-officers")]
    public async Task<IActionResult> GetAccountingOfficers(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(Role: "ChiefAccountant", PageSize: 200), ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardV1Controller(IMediator mediator) => _mediator = mediator;

    /// <summary>Get role-aware dashboard statistics</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DashboardStatsQuery(), ct);
        return Ok(result);
    }
}

public record SubmitProposalRequest(
    int SignaturePageNumber,
    decimal SignaturePositionX,
    decimal SignaturePositionY,
    decimal SignatureWidth,
    decimal SignatureHeight,
    decimal SignatureRotation,
    string GeneratedPdfPath);
