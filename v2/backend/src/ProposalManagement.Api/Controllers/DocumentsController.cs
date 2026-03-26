using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Documents;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/documents")]
public class DocumentsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId, [FromQuery] int? tab = null)
        => ToActionResult(await Mediator.Send(new GetProposalDocumentsQuery(proposalId, tab)));

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(Guid proposalId, [FromForm] IFormFile file,
        [FromForm] int tabNumber, [FromForm] string documentType, [FromForm] string? docName)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var command = new UploadDocumentCommand
        {
            ProposalId = proposalId, TabNumber = tabNumber, DocumentType = documentType,
            DocName = docName, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        };
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid proposalId, Guid id)
        => ToActionResult(await Mediator.Send(new DeleteDocumentCommand(id)));
}
