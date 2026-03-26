using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Api.Controllers.V1;

[ApiController]
[Route("api/v1/proposals/{proposalId:guid}/documents")]
[Authorize]
public class DocumentsV1Controller : ControllerBase
{
    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IRepository<ProposalDocument> _docRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public DocumentsV1Controller(
        IRepository<Proposal> proposalRepo,
        IRepository<ProposalDocument> docRepo,
        ICurrentUser currentUser,
        IAuditService auditService,
        IWebHostEnvironment env)
    {
        _proposalRepo = proposalRepo;
        _docRepo = docRepo;
        _currentUser = currentUser;
        _auditService = auditService;
        _env = env;
    }

    /// <summary>Upload a document for a proposal</summary>
    [HttpPost]
    [Authorize(Roles = "Submitter,CityEngineer,ADO,ChiefAccountant,Lotus")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        Guid proposalId,
        [FromForm] IFormFile file,
        [FromForm] string documentType,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (file.Length > MaxFileSize)
            return BadRequest(new { error = "File size exceeds 10 MB limit" });

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { error = "Only PDF, JPG, and PNG files are allowed" });

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Invalid content type" });

        // Validate file header bytes (magic numbers)
        if (!await ValidateFileHeader(file, ct))
            return BadRequest(new { error = "File content does not match declared type" });

        if (!Enum.TryParse<DocumentType>(documentType, true, out var docType))
            return BadRequest(new { error = $"Invalid document type: {documentType}" });

        var proposal = await _proposalRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == proposalId, ct);

        if (proposal is null)
            return NotFound(new { error = "Proposal not found" });

        // Access control
        var isLotus = _currentUser.Role == UserRole.Lotus;
        if (!isLotus && proposal.SubmittedById != _currentUser.UserId)
            return StatusCode(403, new { error = "Access denied" });

        // Store file on disk
        var uploadsRoot = Path.Combine(_env.ContentRootPath, "Uploads", "Documents");
        Directory.CreateDirectory(uploadsRoot);
        var safeFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsRoot, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var doc = new ProposalDocument
        {
            Id = Guid.NewGuid(),
            ProposalId = proposalId,
            DocumentType = docType,
            FileName = Path.GetFileName(file.FileName),
            FileSize = file.Length,
            ContentType = file.ContentType,
            StoragePath = $"Uploads/Documents/{safeFileName}",
            UploadedById = _currentUser.UserId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _docRepo.AddAsync(doc, ct);

        await _auditService.LogAsync(
            AuditAction.Upload, "ProposalDocument", doc.Id.ToString(),
            $"Document '{file.FileName}' ({docType}) uploaded for proposal {proposal.ProposalNumber}",
            AuditModule.Document, AuditSeverity.Info,
            cancellationToken: ct);

        return Ok(new
        {
            id = doc.Id,
            fileName = doc.FileName,
            documentType = docType.ToString(),
            fileSize = doc.FileSize
        });
    }

    /// <summary>Download a proposal document</summary>
    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> Download(Guid proposalId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await _docRepo.Query()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ProposalId == proposalId, ct);

        if (doc is null)
            return NotFound(new { error = "Document not found" });

        var filePath = Path.Combine(_env.ContentRootPath, doc.StoragePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "File not found on disk" });

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(stream, doc.ContentType, doc.FileName);
    }

    /// <summary>Delete (soft) a proposal document</summary>
    [HttpDelete("{documentId:guid}")]
    [Authorize(Roles = "Submitter,Lotus")]
    public async Task<IActionResult> Delete(Guid proposalId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await _docRepo.Query()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ProposalId == proposalId, ct);

        if (doc is null)
            return NotFound(new { error = "Document not found" });

        if (_currentUser.Role != UserRole.Lotus && doc.UploadedById != _currentUser.UserId)
            return StatusCode(403, new { error = "Access denied" });

        doc.IsDeleted = true;
        await _docRepo.UpdateAsync(doc, ct);

        return NoContent();
    }

    /// <summary>Validate file header bytes match declared content type</summary>
    private static async Task<bool> ValidateFileHeader(IFormFile file, CancellationToken ct)
    {
        var buffer = new byte[8];
        using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer, ct);
        if (bytesRead < 4) return false;

        // PDF: %PDF
        if (file.ContentType == "application/pdf")
            return buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46;

        // JPEG: FF D8 FF
        if (file.ContentType == "image/jpeg")
            return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;

        // PNG: 89 50 4E 47
        if (file.ContentType == "image/png")
            return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;

        return false;
    }
}
