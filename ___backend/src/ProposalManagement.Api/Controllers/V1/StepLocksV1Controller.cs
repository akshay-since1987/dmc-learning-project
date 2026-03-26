using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.V1.StepLocks;

namespace ProposalManagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/proposals/{proposalId:guid}/steps/{stepNumber:int}/lock")]
[Authorize]
public class StepLocksV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    public StepLocksV1Controller(IMediator mediator) => _mediator = mediator;

    /// <summary>Check lock status for a step</summary>
    [HttpGet]
    public async Task<IActionResult> GetLock(Guid proposalId, int stepNumber, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetStepLockQuery(proposalId, stepNumber), ct);
        return result is null ? Ok(new { locked = false }) : Ok(new { locked = true, lockInfo = result });
    }

    /// <summary>Acquire a lock on a shared step</summary>
    [HttpPost]
    public async Task<IActionResult> AcquireLock(Guid proposalId, int stepNumber, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new AcquireStepLockCommand(proposalId, stepNumber), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Release a lock on a shared step</summary>
    [HttpDelete]
    public async Task<IActionResult> ReleaseLock(Guid proposalId, int stepNumber, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ReleaseStepLockCommand(proposalId, stepNumber), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }
}
