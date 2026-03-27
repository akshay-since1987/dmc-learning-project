using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Workflow.Commands;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class WorkflowController : BaseController
{
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id)
        => ToActionResult(await Mediator.Send(new SubmitProposalCommand(id)));

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveProposalCommand command)
    {
        if (id != command.ProposalId) return BadRequest(new { success = false, error = "ID mismatch" });
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpPost("{id:guid}/pushback")]
    public async Task<IActionResult> PushBack(Guid id, [FromBody] PushBackProposalCommand command)
    {
        if (id != command.ProposalId) return BadRequest(new { success = false, error = "ID mismatch" });
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpPost("{id:guid}/approval-signature")]
    public async Task<IActionResult> UploadApprovalSignature(Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadApprovalSignatureCommand
        {
            ProposalId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }
}
