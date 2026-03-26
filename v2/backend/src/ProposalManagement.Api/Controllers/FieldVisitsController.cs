using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.FieldVisits;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/field-visits")]
public class FieldVisitsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetFieldVisitsQuery(proposalId)));

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid proposalId, [FromBody] AssignFieldVisitRequest request)
        => ToActionResult(await Mediator.Send(new AssignFieldVisitCommand(proposalId, request.AssignedToId)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid proposalId, Guid id, [FromBody] UpdateFieldVisitCommand command)
    {
        if (id != command.Id) return BadRequest(new { success = false, error = "ID mismatch" });
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid proposalId, Guid id)
        => ToActionResult(await Mediator.Send(new CompleteFieldVisitCommand(id)));
}

public record AssignFieldVisitRequest(Guid AssignedToId);
