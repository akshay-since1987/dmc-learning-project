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

    [HttpPost("{id:guid}/pdf")]
    public async Task<IActionResult> UploadPdf(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadTsPdfCommand
        {
            TsId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }

    [HttpPost("{id:guid}/outside-approval-letter")]
    public async Task<IActionResult> UploadOutsideApprovalLetter(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadOutsideApprovalLetterCommand
        {
            TsId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }

    [HttpPost("{id:guid}/signer-signature")]
    public async Task<IActionResult> UploadSignerSignature(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadSignerSignatureCommand
        {
            TsId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }
}
