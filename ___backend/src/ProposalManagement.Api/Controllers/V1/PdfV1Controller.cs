using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.V1.Pdf;

namespace ProposalManagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/proposals/{proposalId:guid}/pdf")]
[Authorize]
public class PdfV1Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorage _fileStorage;

    public PdfV1Controller(IMediator mediator, IFileStorage fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    /// <summary>Generate a stage-note PDF for the proposal at its current stage</summary>
    [HttpPost("generate")]
    [Authorize(Roles = "Submitter,CityEngineer,ADO,ChiefAccountant,DeputyCommissioner,Commissioner,Lotus")]
    public async Task<IActionResult> Generate(Guid proposalId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GenerateStagePdfCommand(proposalId), ct);
        return result.IsSuccess
            ? Ok(new { documentId = result.Data!.DocumentId, filePath = result.Data.FilePath })
            : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Download a generated PDF by its storage path</summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { error = "Path is required" });

        // Prevent path traversal
        if (path.Contains("..") || Path.IsPathRooted(path))
            return BadRequest(new { error = "Invalid path" });

        if (!await _fileStorage.ExistsAsync(path, ct))
            return NotFound(new { error = "File not found" });

        var stream = await _fileStorage.GetAsync(path, ct);
        return File(stream, "application/pdf", Path.GetFileName(path));
    }

    /// <summary>List all generated PDFs for this proposal</summary>
    [HttpGet]
    public async Task<IActionResult> List(Guid proposalId, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetGeneratedPdfsQuery(proposalId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    /// <summary>Save a signature placement on a generated PDF</summary>
    [HttpPost("sign")]
    [Authorize(Roles = "Submitter,CityEngineer,ADO,ChiefAccountant,DeputyCommissioner,Commissioner,Lotus")]
    public async Task<IActionResult> Sign(Guid proposalId, [FromBody] SignRequest body, CancellationToken ct = default)
    {
        var command = new SaveSignatureCommand(
            proposalId,
            body.StageHistoryId,
            body.PageNumber,
            body.PositionX,
            body.PositionY,
            body.Width,
            body.Height,
            body.Rotation,
            body.GeneratedPdfPath);

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { signatureId = result.Data })
            : StatusCode(result.StatusCode, new { result.Error });
    }
}

public record SignRequest(
    long StageHistoryId,
    int PageNumber,
    decimal PositionX,
    decimal PositionY,
    decimal Width,
    decimal Height,
    decimal Rotation,
    string GeneratedPdfPath);
