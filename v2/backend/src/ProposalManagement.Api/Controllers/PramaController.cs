using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Prama;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/prama")]
public class PramaController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetPramaDetailQuery(proposalId)));

    [HttpPost]
    public async Task<IActionResult> Save(Guid proposalId, [FromBody] SavePramaDetailCommand command)
        => ToActionResult(await Mediator.Send(command with { ProposalId = proposalId }));
}
