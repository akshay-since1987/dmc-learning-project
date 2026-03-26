using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.TechnicalSanctions;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/technical-sanction")]
public class TechnicalSanctionsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetTechnicalSanctionQuery(proposalId)));

    [HttpPost]
    public async Task<IActionResult> Save(Guid proposalId, [FromBody] SaveTechnicalSanctionCommand command)
        => ToActionResult(await Mediator.Send(command with { ProposalId = proposalId }));

    [HttpPost("{id:guid}/sign")]
    public async Task<IActionResult> Sign(Guid proposalId, Guid id)
        => ToActionResult(await Mediator.Send(new SignTechnicalSanctionCommand(id)));
}
