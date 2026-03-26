using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Proposals.Commands;
using ProposalManagement.Application.Proposals.Queries;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class ProposalsController : BaseController
{
    [HttpGet("my")]
    public async Task<IActionResult> GetMyProposals([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? stage = null, [FromQuery] string? search = null)
        => ToActionResult(await Mediator.Send(new GetMyProposalsQuery(page, pageSize, stage, search)));

    [HttpGet("all")]
    public async Task<IActionResult> GetAllProposals([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? stage = null, [FromQuery] string? search = null, [FromQuery] Guid? departmentId = null)
        => ToActionResult(await Mediator.Send(new GetAllProposalsQuery(page, pageSize, stage, search, departmentId)));

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => ToActionResult(await Mediator.Send(new GetPendingApprovalsQuery(page, pageSize)));

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => ToActionResult(await Mediator.Send(new GetProposalStatsQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => ToActionResult(await Mediator.Send(new GetProposalByIdQuery(id)));

    [HttpGet("{id:guid}/approval-history")]
    public async Task<IActionResult> GetApprovalHistory(Guid id)
        => ToActionResult(await Mediator.Send(new GetApprovalHistoryQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProposalCommand command)
        => ToActionResult(await Mediator.Send(command));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProposalCommand command)
    {
        if (id != command.Id) return BadRequest(new { success = false, error = "ID mismatch" });
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => ToActionResult(await Mediator.Send(new DeleteProposalCommand(id)));
}
