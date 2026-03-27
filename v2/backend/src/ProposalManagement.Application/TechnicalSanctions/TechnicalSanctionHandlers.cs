using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.TechnicalSanctions;

// ── DTOs ──
public record TechnicalSanctionDto
{
    public Guid Id { get; init; }
    public string? TsNumber { get; init; }
    public DateTime? TsDate { get; init; }
    public decimal? TsAmount { get; init; }
    public string? Description_En { get; init; }
    public string? Description_Mr { get; init; }
    public string? TsPdfPath { get; init; }
    public string? OutsideApprovalLetterPath { get; init; }
    public string? SanctionedByName { get; init; }
    public string? SanctionedByName_Mr { get; init; }
    public string? SanctionedByDept { get; init; }
    public string? SanctionedByDept_Mr { get; init; }
    public string? SanctionedByDesignation { get; init; }
    public string? SanctionedByDesignation_Mr { get; init; }
    public string? PreparedByName { get; init; }
    public string? SignedByName { get; init; }
    public string? SignerSignaturePath { get; init; }
    public DateTime? SignedAt { get; init; }
    public string Status { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}

// ── Query ──
public record GetTechnicalSanctionQuery(Guid ProposalId) : IRequest<Result<TechnicalSanctionDto?>>;

public class GetTechnicalSanctionHandler(IAppDbContext db) : IRequestHandler<GetTechnicalSanctionQuery, Result<TechnicalSanctionDto?>>
{
    public async Task<Result<TechnicalSanctionDto?>> Handle(GetTechnicalSanctionQuery request, CancellationToken ct)
    {
        var ts = await db.TechnicalSanctions
            .Where(t => t.ProposalId == request.ProposalId && !t.IsDeleted)
            .Include(t => t.PreparedBy).Include(t => t.SignedBy)
            .FirstOrDefaultAsync(ct);

        if (ts is null) return Result<TechnicalSanctionDto?>.Success(null);
        return Result<TechnicalSanctionDto?>.Success(new TechnicalSanctionDto
        {
            Id = ts.Id, TsNumber = ts.TsNumber, TsDate = ts.TsDate, TsAmount = ts.TsAmount,
            Description_En = ts.Description_En, Description_Mr = ts.Description_Mr,
            TsPdfPath = ts.TsPdfPath, OutsideApprovalLetterPath = ts.OutsideApprovalLetterPath,
            SanctionedByName = ts.SanctionedByName, SanctionedByName_Mr = ts.SanctionedByName_Mr,
            SanctionedByDept = ts.SanctionedByDept, SanctionedByDept_Mr = ts.SanctionedByDept_Mr,
            SanctionedByDesignation = ts.SanctionedByDesignation, SanctionedByDesignation_Mr = ts.SanctionedByDesignation_Mr,
            PreparedByName = ts.PreparedBy?.FullName_En, SignedByName = ts.SignedBy?.FullName_En,
            SignerSignaturePath = ts.SignerSignaturePath, SignedAt = ts.SignedAt,
            Status = ts.Status, CreatedAt = ts.CreatedAt
        });
    }
}

// ── Commands ──
public record SaveTechnicalSanctionCommand : IRequest<Result<Guid>>
{
    public Guid ProposalId { get; init; }
    public string? TsNumber { get; init; }
    public DateTime? TsDate { get; init; }
    public decimal? TsAmount { get; init; }
    public string? Description_En { get; init; }
    public string? Description_Mr { get; init; }
    public string? SanctionedByName { get; init; }
    public string? SanctionedByName_Mr { get; init; }
    public string? SanctionedByDept { get; init; }
    public string? SanctionedByDept_Mr { get; init; }
    public string? SanctionedByDesignation { get; init; }
    public string? SanctionedByDesignation_Mr { get; init; }
}

public class SaveTechnicalSanctionHandler(IAppDbContext db, ICurrentUser user, ILogger<SaveTechnicalSanctionHandler> logger) 
    : IRequestHandler<SaveTechnicalSanctionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveTechnicalSanctionCommand request, CancellationToken ct)
    {
        var existing = await db.TechnicalSanctions.FirstOrDefaultAsync(t => t.ProposalId == request.ProposalId && !t.IsDeleted, ct);

        if (existing is not null)
        {
            existing.TsNumber = request.TsNumber;
            existing.TsDate = request.TsDate;
            existing.TsAmount = request.TsAmount;
            existing.Description_En = request.Description_En;
            existing.Description_Mr = request.Description_Mr;
            existing.SanctionedByName = request.SanctionedByName;
            existing.SanctionedByName_Mr = request.SanctionedByName_Mr;
            existing.SanctionedByDept = request.SanctionedByDept;
            existing.SanctionedByDept_Mr = request.SanctionedByDept_Mr;
            existing.SanctionedByDesignation = request.SanctionedByDesignation;
            existing.SanctionedByDesignation_Mr = request.SanctionedByDesignation_Mr;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var ts = new TechnicalSanction
        {
            Id = Guid.NewGuid(), ProposalId = request.ProposalId,
            TsNumber = request.TsNumber, TsDate = request.TsDate, TsAmount = request.TsAmount,
            Description_En = request.Description_En, Description_Mr = request.Description_Mr,
            SanctionedByName = request.SanctionedByName, SanctionedByName_Mr = request.SanctionedByName_Mr,
            SanctionedByDept = request.SanctionedByDept, SanctionedByDept_Mr = request.SanctionedByDept_Mr,
            SanctionedByDesignation = request.SanctionedByDesignation, SanctionedByDesignation_Mr = request.SanctionedByDesignation_Mr,
            PreparedById = user.UserId, Status = nameof(TechnicalSanctionStatus.Draft),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.TechnicalSanctions.Add(ts);

        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is not null && proposal.CompletedTab < 4) proposal.CompletedTab = 4;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Technical sanction saved for Proposal {ProposalId}", request.ProposalId);
        return Result<Guid>.Success(ts.Id);
    }
}

public record SignTechnicalSanctionCommand(Guid TsId) : IRequest<Result>;

public class SignTechnicalSanctionHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<SignTechnicalSanctionCommand, Result>
{
    public async Task<Result> Handle(SignTechnicalSanctionCommand request, CancellationToken ct)
    {
        var ts = await db.TechnicalSanctions.Include(t => t.Proposal).FirstOrDefaultAsync(t => t.Id == request.TsId, ct);
        if (ts is null) return Result.NotFound();

        ts.SignedById = user.UserId;
        ts.SignedAt = DateTime.UtcNow;
        ts.Status = nameof(TechnicalSanctionStatus.Signed);
        ts.Proposal.CurrentStage = nameof(ProposalStage.TechnicalSanctionComplete);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Upload TS PDF ──
public record UploadTsPdfCommand : IRequest<Result>
{
    public Guid TsId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadTsPdfHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadTsPdfHandler> logger)
    : IRequestHandler<UploadTsPdfCommand, Result>
{
    private const long MaxPdfSize = 10 * 1024 * 1024; // 10 MB

    public async Task<Result> Handle(UploadTsPdfCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxPdfSize) return Result.Failure("PDF size exceeds 10 MB limit");
        if (!string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Only PDF files are allowed");

        var ts = await db.TechnicalSanctions.Include(t => t.Proposal).FirstOrDefaultAsync(t => t.Id == request.TsId, ct);
        if (ts is null) return Result.NotFound("Technical sanction not found");

        var folder = Path.Combine("wwwroot", "uploads", "technical-sanctions", ts.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(ts.TsPdfPath))
        {
            var oldFile = Path.Combine("wwwroot", ts.TsPdfPath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        ts.TsPdfPath = $"/uploads/technical-sanctions/{ts.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("TS PDF {FileName} uploaded for TechnicalSanction {TsId}", safeFileName, request.TsId);
        return Result.Success();
    }
}

// ── Upload Outside Approval Letter ──
public record UploadOutsideApprovalLetterCommand : IRequest<Result>
{
    public Guid TsId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadOutsideApprovalLetterHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadOutsideApprovalLetterHandler> logger)
    : IRequestHandler<UploadOutsideApprovalLetterCommand, Result>
{
    private const long MaxSize = 10 * 1024 * 1024; // 10 MB

    public async Task<Result> Handle(UploadOutsideApprovalLetterCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxSize) return Result.Failure("File size exceeds 10 MB limit");

        var ts = await db.TechnicalSanctions.Include(t => t.Proposal).FirstOrDefaultAsync(t => t.Id == request.TsId, ct);
        if (ts is null) return Result.NotFound("Technical sanction not found");

        var folder = Path.Combine("wwwroot", "uploads", "technical-sanctions", ts.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(ts.OutsideApprovalLetterPath))
        {
            var oldFile = Path.Combine("wwwroot", ts.OutsideApprovalLetterPath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        ts.OutsideApprovalLetterPath = $"/uploads/technical-sanctions/{ts.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Outside approval letter {FileName} uploaded for TechnicalSanction {TsId}", safeFileName, request.TsId);
        return Result.Success();
    }
}

// ── Upload Signer Signature ──
public record UploadSignerSignatureCommand : IRequest<Result>
{
    public Guid TsId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadSignerSignatureHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadSignerSignatureHandler> logger)
    : IRequestHandler<UploadSignerSignatureCommand, Result>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/svg+xml" };
    private const long MaxSize = 2 * 1024 * 1024; // 2 MB

    public async Task<Result> Handle(UploadSignerSignatureCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxSize) return Result.Failure("Signature file exceeds 2 MB limit");
        if (!AllowedTypes.Contains(request.ContentType)) return Result.Failure("Signature must be PNG, JPEG, or SVG");

        var ts = await db.TechnicalSanctions.Include(t => t.Proposal).FirstOrDefaultAsync(t => t.Id == request.TsId, ct);
        if (ts is null) return Result.NotFound("Technical sanction not found");

        var folder = Path.Combine("wwwroot", "uploads", "technical-sanctions", ts.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(ts.SignerSignaturePath))
        {
            var oldFile = Path.Combine("wwwroot", ts.SignerSignaturePath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var storageName = $"{Guid.NewGuid():N}_signer_signature.png";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        ts.SignerSignaturePath = $"/uploads/technical-sanctions/{ts.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Signer signature uploaded for TechnicalSanction {TsId}", request.TsId);
        return Result.Success();
    }
}
