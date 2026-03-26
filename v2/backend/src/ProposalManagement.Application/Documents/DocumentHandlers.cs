using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Documents;

// ── DTOs ──
public record ProposalDocumentDto(Guid Id, Guid ProposalId, int TabNumber, string DocumentType, string? DocName,
    string FileName, long FileSize, string ContentType, string? UploadedByName, DateTime CreatedAt);

// ── Query ──
public record GetProposalDocumentsQuery(Guid ProposalId, int? TabNumber = null) : IRequest<Result<List<ProposalDocumentDto>>>;

public class GetProposalDocumentsHandler(IAppDbContext db) : IRequestHandler<GetProposalDocumentsQuery, Result<List<ProposalDocumentDto>>>
{
    public async Task<Result<List<ProposalDocumentDto>>> Handle(GetProposalDocumentsQuery request, CancellationToken ct)
    {
        var q = db.ProposalDocuments
            .Where(d => d.ProposalId == request.ProposalId && !d.IsDeleted)
            .Include(d => d.UploadedBy)
            .AsQueryable();

        if (request.TabNumber.HasValue)
            q = q.Where(d => d.TabNumber == request.TabNumber.Value);

        var items = await q.OrderBy(d => d.CreatedAt)
            .Select(d => new ProposalDocumentDto(d.Id, d.ProposalId, d.TabNumber, d.DocumentType,
                d.DocName, d.FileName, d.FileSize, d.ContentType, d.UploadedBy.FullName_En, d.CreatedAt))
            .ToListAsync(ct);

        return Result<List<ProposalDocumentDto>>.Success(items);
    }
}

// ── Upload command ──
public record UploadDocumentCommand : IRequest<Result<Guid>>
{
    public Guid ProposalId { get; init; }
    public int TabNumber { get; init; }
    public string DocumentType { get; init; } = default!;
    public string? DocName { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadDocumentHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadDocumentHandler> logger) 
    : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/jpeg", "image/png", "image/gif",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public async Task<Result<Guid>> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxFileSize) return Result<Guid>.Failure("File size exceeds 10 MB limit");
        if (!AllowedTypes.Contains(request.ContentType)) return Result<Guid>.Failure($"File type '{request.ContentType}' is not allowed");

        // Save to wwwroot/uploads/{proposalId}/{guid}_{filename}
        var safeFileName = Path.GetFileName(request.FileName);
        var folder = Path.Combine("wwwroot", "uploads", request.ProposalId.ToString());
        Directory.CreateDirectory(folder);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        var doc = new ProposalDocument
        {
            Id = Guid.NewGuid(), ProposalId = request.ProposalId, TabNumber = request.TabNumber,
            DocumentType = request.DocumentType, DocName = request.DocName,
            FileName = safeFileName, FileSize = request.FileSize, ContentType = request.ContentType,
            StoragePath = $"/uploads/{request.ProposalId}/{storageName}",
            UploadedById = user.UserId!.Value,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.ProposalDocuments.Add(doc);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Document {FileName} uploaded for Proposal {ProposalId}", safeFileName, request.ProposalId);
        return Result<Guid>.Success(doc.Id);
    }
}

// ── Delete document command ──
public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result>;

public class DeleteDocumentHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<DeleteDocumentCommand, Result>
{
    public async Task<Result> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        var doc = await db.ProposalDocuments.FindAsync(new object[] { request.DocumentId }, ct);
        if (doc is null) return Result.NotFound();

        doc.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
