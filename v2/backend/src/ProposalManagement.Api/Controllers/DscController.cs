using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Dsc;

namespace ProposalManagement.Api.Controllers;

[Authorize]
public class DscController : BaseController
{
    [HttpPost("sign")]
    public async Task<IActionResult> Sign([FromBody] SignDocumentRequest request)
        => ToActionResult(await Mediator.Send(new SignDocumentCommand(request.ProposalId, request.DocumentPath)));

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifySignatureRequest request)
        => ToActionResult(await Mediator.Send(new VerifySignatureCommand(request.DocumentPath, request.SignatureReference)));
}

public record SignDocumentRequest(Guid ProposalId, string DocumentPath);
public record VerifySignatureRequest(string DocumentPath, string SignatureReference);
