using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Pdf;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class PdfController : BaseController
{
    [HttpPost("{proposalId}/generate")]
    public async Task<IActionResult> Generate(Guid proposalId, [FromBody] GeneratePdfRequest request)
        => ToActionResult(await Mediator.Send(new GeneratePdfCommand(proposalId, request.PdfType)));

    [HttpGet("{proposalId}/list")]
    public async Task<IActionResult> List(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetGeneratedPdfsQuery(proposalId)));
}

public record GeneratePdfRequest(string PdfType);
