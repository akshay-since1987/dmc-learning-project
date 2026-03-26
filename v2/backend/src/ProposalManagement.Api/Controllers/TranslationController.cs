using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Api.Controllers;

[ApiController]
[Route("api/translation")]
[Authorize]
public class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;

    public TranslationController(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Text is required" });

        var result = await _translationService.TranslateAsync(
            request.Text, request.SourceLang ?? "en", request.TargetLang ?? "mr", ct);

        return Ok(new { translatedText = result });
    }

    [HttpPost("translate-batch")]
    public async Task<IActionResult> TranslateBatch([FromBody] TranslateBatchRequest request, CancellationToken ct)
    {
        if (request.Texts == null || request.Texts.Count == 0)
            return BadRequest(new { message = "Texts array is required" });

        var results = await _translationService.TranslateBatchAsync(
            request.Texts, request.SourceLang ?? "en", request.TargetLang ?? "mr", ct);

        return Ok(new { translatedTexts = results });
    }
}

public record TranslateRequest(string Text, string? SourceLang, string? TargetLang);
public record TranslateBatchRequest(List<string> Texts, string? SourceLang, string? TargetLang);
