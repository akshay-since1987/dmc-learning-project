using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Budget;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/budget")]
public class BudgetController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetBudgetDetailQuery(proposalId)));

    [HttpPost]
    public async Task<IActionResult> Save(Guid proposalId, [FromBody] SaveBudgetDetailCommand command)
        => ToActionResult(await Mediator.Send(command with { ProposalId = proposalId }));
}
