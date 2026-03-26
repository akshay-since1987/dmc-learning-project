using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Auth.Commands;
using ProposalManagement.Application.Auth.Queries;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUser _currentUser;

    public AuthController(IMediator mediator, IWebHostEnvironment env, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _env = env;
        _currentUser = currentUser;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(new { message = "OTP sent successfully" }) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }

    [Authorize]
    [HttpPost("me/signature")]
    public async Task<IActionResult> UploadMySignature([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Signature file is required." });

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Only PNG, JPEG, and WebP images are allowed." });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Signature file must be under 2 MB." });

        if (_currentUser.UserId == Guid.Empty)
            return Unauthorized(new { message = "Unauthorized" });

        var sigDir = Path.Combine(_env.WebRootPath, "images", "signatures");
        Directory.CreateDirectory(sigDir);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
            ext = ".png";

        var fileName = $"{_currentUser.UserId}{ext}";
        var filePath = Path.Combine(sigDir, fileName);

        foreach (var old in Directory.GetFiles(sigDir, $"{_currentUser.UserId}.*"))
        {
            System.IO.File.Delete(old);
        }

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/images/signatures/{fileName}";
        var result = await _mediator.Send(new UpdateMySignatureCommand(relativePath), cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Error });
    }
}
