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

    [HttpPost("{id:guid}/pdf")]
    public async Task<IActionResult> UploadPdf(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadEstimatePdfCommand
        {
            EstimateId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }

    [HttpPost("{id:guid}/prepared-signature")]
    public async Task<IActionResult> UploadPreparedSignature(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadPreparedSignatureCommand
        {
            EstimateId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }

    [HttpPost("{id:guid}/approver-signature")]
    public async Task<IActionResult> UploadApproverSignature(Guid proposalId, Guid id, [FromForm] IFormFile file)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ToActionResult(await Mediator.Send(new UploadApproverSignatureCommand
        {
            EstimateId = id, FileName = file.FileName, FileSize = file.Length,
            ContentType = file.ContentType, FileContent = ms.ToArray()
        }));
    }

    [HttpPost("{id:guid}/sign-pdf")]
    public async Task<IActionResult> SignPdf(Guid proposalId, Guid id, [FromBody] SignEstimatePdfRequest request)
        => ToActionResult(await Mediator.Send(new SignEstimatePdfCommand
        {
            EstimateId = id,
            SignatureType = request.SignatureType,
            PageNumber = request.PageNumber,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Width = request.Width,
            Height = request.Height,
            Rotation = request.Rotation
        }));
}

public record SendEstimateForApprovalRequest(string TargetRole);
public record ReturnEstimateRequest(string QueryNote_En);
public record SignEstimatePdfRequest(string SignatureType, int PageNumber, decimal PositionX, decimal PositionY, decimal Width, decimal Height, decimal Rotation);
