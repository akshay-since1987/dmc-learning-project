using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Lotus.Commands;
using ProposalManagement.Application.Lotus.Queries;

namespace ProposalManagement.Api.Controllers.Lotus;

[ApiController]
[Route("api/lotus/users")]
[Authorize(Roles = "Lotus")]
public class LotusUsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public LotusUsersController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? role, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUsersQuery(search, role, pageIndex, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data }) : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken ct = default)
    {
        if (id != command.Id) return BadRequest(new { message = "ID mismatch" });
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { result.Error });
    }

    [HttpPost("{id:guid}/signature")]
    public async Task<IActionResult> UploadSignature(Guid id, IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Signature file is required." });

        // Validate file type (images only)
        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Only PNG, JPEG, and WebP images are allowed." });

        // Max 2 MB
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Signature file must be under 2 MB." });

        var sigDir = Path.Combine(_env.WebRootPath, "images", "signatures");
        Directory.CreateDirectory(sigDir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext)) ext = ".png";
        var fileName = $"{id}{ext}";
        var filePath = Path.Combine(sigDir, fileName);

        // Remove old signature files for this user (different extension)
        foreach (var old in Directory.GetFiles(sigDir, $"{id}.*"))
        {
            System.IO.File.Delete(old);
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relativePath = $"/images/signatures/{fileName}";

        var updateResult = await _mediator.Send(new UpdateUserSignatureCommand(id, relativePath), ct);
        if (!updateResult.IsSuccess)
            return StatusCode(updateResult.StatusCode, new { updateResult.Error });

        return Ok(new { signaturePath = relativePath });
    }
}
