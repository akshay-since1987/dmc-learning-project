using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Workflow.Commands;
using ProposalManagement.Application.Workflow.Queries;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/workflow")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IMediator _mediator;
    public WorkflowController(IMediator mediator) => _mediator = mediator;

    /// <summary>Approve a proposal at its current stage</summary>
    [HttpPost("{proposalId:guid}/approve")]
    [Authorize(Roles = "CityEngineer,ADO,ChiefAccountant,DeputyCommissioner,Commissioner,Lotus")]
    public async Task<IActionResult> Approve(Guid proposalId, [FromBody] ApproveRequest body, CancellationToken ct = default)
    {
        var command = new ApproveStageCommand(
            proposalId,
            body.TermsAccepted,
            body.Opinion_En, body.Opinion_Alt,
            body.Remarks_En, body.Remarks_Alt);
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { message = "Proposal approved successfully", historyId = result.Data })
            : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Push back a proposal to an earlier stage with mandatory reason</summary>
    [HttpPost("{proposalId:guid}/pushback")]
    [Authorize(Roles = "CityEngineer,ADO,ChiefAccountant,DeputyCommissioner,Commissioner,Lotus")]
    public async Task<IActionResult> PushBack(Guid proposalId, [FromBody] PushBackRequest body, CancellationToken ct = default)
    {
        var command = new PushBackStageCommand(
            proposalId,
            body.TargetStage,
            body.Reason_En, body.Reason_Alt,
            body.Opinion_En, body.Opinion_Alt,
            body.Remarks_En, body.Remarks_Alt);
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { message = "Proposal pushed back successfully" })
            : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Get full approval history (timeline) for a proposal</summary>
    [HttpGet("{proposalId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid proposalId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetApprovalHistoryQuery(proposalId), ct);
        return Ok(result);
    }
}

// ── Request DTOs (kept controller-local, thin) ─────────────────────

public record ApproveRequest(
    bool TermsAccepted,
    string? Opinion_En = null,
    string? Opinion_Alt = null,
    string? Remarks_En = null,
    string? Remarks_Alt = null);

public record PushBackRequest(
    string TargetStage = "",
    string Reason_En = "",
    string? Reason_Alt = null,
    string? Opinion_En = null,
    string? Opinion_Alt = null,
    string? Remarks_En = null,
    string? Remarks_Alt = null);
