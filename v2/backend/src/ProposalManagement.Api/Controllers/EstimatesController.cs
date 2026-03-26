using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Estimates;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/estimate")]
public class EstimatesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetEstimateQuery(proposalId)));

    [HttpPost]
    public async Task<IActionResult> Save(Guid proposalId, [FromBody] SaveEstimateCommand command)
        => ToActionResult(await Mediator.Send(command with { ProposalId = proposalId }));

    [HttpPost("{id:guid}/send-for-approval")]
    public async Task<IActionResult> SendForApproval(Guid proposalId, Guid id, [FromBody] SendEstimateForApprovalRequest request)
        => ToActionResult(await Mediator.Send(new SendEstimateForApprovalCommand(id, request.TargetRole)));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid proposalId, Guid id, [FromBody] ApproveEstimateCommand command)
        => ToActionResult(await Mediator.Send(command with { EstimateId = id }));

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid proposalId, Guid id, [FromBody] ReturnEstimateRequest request)
        => ToActionResult(await Mediator.Send(new ReturnEstimateCommand(id, request.QueryNote_En)));
}

public record SendEstimateForApprovalRequest(string TargetRole);
public record ReturnEstimateRequest(string QueryNote_En);
