using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.FieldVisits;

namespace ProposalManagement.Api.Controllers;

[Authorize]
[Route("api/proposals/{proposalId:guid}/field-visits")]
public class FieldVisitsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetFieldVisitsQuery(proposalId)));

    [HttpGet("assignable-engineers")]
    public async Task<IActionResult> GetAssignableEngineers(Guid proposalId)
        => ToActionResult(await Mediator.Send(new GetAssignableEngineersQuery(proposalId)));

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid proposalId, [FromBody] AssignFieldVisitRequest request)
        => ToActionResult(await Mediator.Send(new AssignFieldVisitCommand(proposalId, request.AssignedToId)));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid proposalId, Guid id, [FromBody] UpdateFieldVisitCommand command)
    {
        if (id != command.Id) return BadRequest(new { success = false, error = "ID mismatch" });
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid proposalId, Guid id)
        => ToActionResult(await Mediator.Send(new CompleteFieldVisitCommand(id)));

    [HttpPost("{id:guid}/photos")]
    public async Task<IActionResult> UploadPhoto(Guid proposalId, Guid id,
        [FromForm] IFormFile file, [FromForm] string? caption)
    {
        if (file.Length == 0) return BadRequest(new { success = false, error = "File is empty" });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var command = new UploadFieldVisitPhotoCommand
        {
            FieldVisitId = id,
            FileName = file.FileName,
            FileSize = file.Length,
            ContentType = file.ContentType,
            FileContent = ms.ToArray(),
            Caption = caption
        };
        return ToActionResult(await Mediator.Send(command));
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid proposalId, Guid id, Guid photoId)
        => ToActionResult(await Mediator.Send(new DeleteFieldVisitPhotoCommand(photoId)));
}

public record AssignFieldVisitRequest(Guid AssignedToId);
