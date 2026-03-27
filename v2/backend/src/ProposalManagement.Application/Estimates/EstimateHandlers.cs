using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Estimates;

// ── DTOs ──
public record EstimateDto
{
    public Guid Id { get; init; }
    public Guid ProposalId { get; init; }
    public string? EstimatePdfPath { get; init; }
    public decimal? EstimatedCost { get; init; }
    public Guid PreparedById { get; init; }
    public string? PreparedByName { get; init; }
    public string? PreparedSignaturePath { get; init; }
    public string? SentToRole { get; init; }
    public string? SentToName { get; init; }
    public string? ApprovedByName { get; init; }
    public string? ApproverSignaturePath { get; init; }
    public bool ApproverDisclaimerAccepted { get; init; }
    public string? ApproverOpinion_En { get; init; }
    public string? ApproverOpinion_Mr { get; init; }
    public string Status { get; init; } = default!;
    public string? ReturnQueryNote_En { get; init; }
    public string? ReturnQueryNote_Mr { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ── Queries ──
public record GetEstimateQuery(Guid ProposalId) : IRequest<Result<EstimateDto?>>;

public class GetEstimateHandler(IAppDbContext db) : IRequestHandler<GetEstimateQuery, Result<EstimateDto?>>
{
    public async Task<Result<EstimateDto?>> Handle(GetEstimateQuery request, CancellationToken ct)
    {
        var est = await db.Estimates
            .Where(e => e.ProposalId == request.ProposalId && !e.IsDeleted)
            .Include(e => e.PreparedBy).Include(e => e.SentTo).Include(e => e.ApprovedBy)
            .FirstOrDefaultAsync(ct);

        if (est is null) return Result<EstimateDto?>.Success(null);
        return Result<EstimateDto?>.Success(new EstimateDto
        {
            Id = est.Id, ProposalId = est.ProposalId, EstimatePdfPath = est.EstimatePdfPath,
            EstimatedCost = est.EstimatedCost, PreparedById = est.PreparedById,
            PreparedByName = est.PreparedBy.FullName_En, PreparedSignaturePath = est.PreparedSignaturePath,
            SentToRole = est.SentToRole, SentToName = est.SentTo?.FullName_En,
            ApprovedByName = est.ApprovedBy?.FullName_En, ApproverSignaturePath = est.ApproverSignaturePath,
            ApproverDisclaimerAccepted = est.ApproverDisclaimerAccepted,
            ApproverOpinion_En = est.ApproverOpinion_En, ApproverOpinion_Mr = est.ApproverOpinion_Mr,
            Status = est.Status,
            ReturnQueryNote_En = est.ReturnQueryNote_En, ReturnQueryNote_Mr = est.ReturnQueryNote_Mr, ApprovedAt = est.ApprovedAt, CreatedAt = est.CreatedAt,
        });
    }
}

// ── Commands ──
public record SaveEstimateCommand : IRequest<Result<Guid>>
{
    public Guid ProposalId { get; init; }
    public decimal? EstimatedCost { get; init; }
}

public class SaveEstimateHandler(IAppDbContext db, ICurrentUser user, ILogger<SaveEstimateHandler> logger)
    : IRequestHandler<SaveEstimateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveEstimateCommand request, CancellationToken ct)
    {
        var existing = await db.Estimates.FirstOrDefaultAsync(e => e.ProposalId == request.ProposalId && !e.IsDeleted, ct);

        if (existing is not null)
        {
            existing.EstimatedCost = request.EstimatedCost;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var est = new Estimate
        {
            Id = Guid.NewGuid(), ProposalId = request.ProposalId, EstimatedCost = request.EstimatedCost,
            PreparedById = user.UserId!.Value, Status = nameof(EstimateStatus.Draft),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.Estimates.Add(est);

        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is not null && proposal.CompletedTab < 3) proposal.CompletedTab = 3;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Estimate saved for Proposal {ProposalId}", request.ProposalId);
        return Result<Guid>.Success(est.Id);
    }
}

public record SendEstimateForApprovalCommand(Guid EstimateId, string TargetRole) : IRequest<Result>;

public class SendEstimateForApprovalHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<SendEstimateForApprovalCommand, Result>
{
    public async Task<Result> Handle(SendEstimateForApprovalCommand request, CancellationToken ct)
    {
        var est = await db.Estimates.Include(e => e.Proposal).FirstOrDefaultAsync(e => e.Id == request.EstimateId, ct);
        if (est is null) return Result.NotFound();

        var target = await db.Users.FirstOrDefaultAsync(u => u.Role == request.TargetRole && u.PalikaId == est.Proposal.PalikaId, ct);
        est.SentToRole = request.TargetRole;
        est.SentToId = target?.Id;
        est.Status = nameof(EstimateStatus.SentForApproval);
        est.Proposal.CurrentStage = nameof(ProposalStage.EstimateSentForApproval);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record ApproveEstimateCommand : IRequest<Result>
{
    public Guid EstimateId { get; init; }
    public bool DisclaimerAccepted { get; init; }
    public string? Opinion_En { get; init; }
    public string? Opinion_Mr { get; init; }
}

public class ApproveEstimateHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<ApproveEstimateCommand, Result>
{
    public async Task<Result> Handle(ApproveEstimateCommand request, CancellationToken ct)
    {
        var est = await db.Estimates.Include(e => e.Proposal).FirstOrDefaultAsync(e => e.Id == request.EstimateId, ct);
        if (est is null) return Result.NotFound();
        if (est.SentToId != user.UserId) return Result.Forbidden("Only the designated approver can approve");

        est.ApprovedById = user.UserId;
        est.ApproverDisclaimerAccepted = request.DisclaimerAccepted;
        est.ApproverOpinion_En = request.Opinion_En;
        est.ApproverOpinion_Mr = request.Opinion_Mr;
        est.Status = nameof(EstimateStatus.Approved);
        est.ApprovedAt = DateTime.UtcNow;
        est.Proposal.CurrentStage = nameof(ProposalStage.EstimateApproved);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record ReturnEstimateCommand(Guid EstimateId, string QueryNote_En, string? QueryNote_Mr = null) : IRequest<Result>;

public class ReturnEstimateHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<ReturnEstimateCommand, Result>
{
    public async Task<Result> Handle(ReturnEstimateCommand request, CancellationToken ct)
    {
        var est = await db.Estimates.FindAsync(new object[] { request.EstimateId }, ct);
        if (est is null) return Result.NotFound();

        est.Status = nameof(EstimateStatus.ReturnedWithQuery);
        est.ReturnQueryNote_En = request.QueryNote_En;
        est.ReturnQueryNote_Mr = request.QueryNote_Mr;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Upload Estimate PDF ──
public record UploadEstimatePdfCommand : IRequest<Result>
{
    public Guid EstimateId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadEstimatePdfHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadEstimatePdfHandler> logger)
    : IRequestHandler<UploadEstimatePdfCommand, Result>
{
    private const long MaxPdfSize = 10 * 1024 * 1024; // 10 MB

    public async Task<Result> Handle(UploadEstimatePdfCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxPdfSize) return Result.Failure("PDF size exceeds 10 MB limit");
        if (!string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            return Result.Failure("Only PDF files are allowed");

        var est = await db.Estimates.Include(e => e.Proposal).FirstOrDefaultAsync(e => e.Id == request.EstimateId, ct);
        if (est is null) return Result.NotFound("Estimate not found");

        var folder = Path.Combine("wwwroot", "uploads", "estimates", est.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(est.EstimatePdfPath))
        {
            var oldFile = Path.Combine("wwwroot", est.EstimatePdfPath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        est.EstimatePdfPath = $"/uploads/estimates/{est.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Estimate PDF {FileName} uploaded for Estimate {EstimateId}", safeFileName, request.EstimateId);
        return Result.Success();
    }
}

// ── Upload Prepared-By Signature ──
public record UploadPreparedSignatureCommand : IRequest<Result>
{
    public Guid EstimateId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadPreparedSignatureHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadPreparedSignatureHandler> logger)
    : IRequestHandler<UploadPreparedSignatureCommand, Result>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/svg+xml" };
    private const long MaxSize = 2 * 1024 * 1024; // 2 MB

    public async Task<Result> Handle(UploadPreparedSignatureCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxSize) return Result.Failure("Signature file exceeds 2 MB limit");
        if (!AllowedTypes.Contains(request.ContentType)) return Result.Failure("Signature must be PNG, JPEG, or SVG");

        var est = await db.Estimates.Include(e => e.Proposal).FirstOrDefaultAsync(e => e.Id == request.EstimateId, ct);
        if (est is null) return Result.NotFound("Estimate not found");

        var folder = Path.Combine("wwwroot", "uploads", "estimates", est.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(est.PreparedSignaturePath))
        {
            var oldFile = Path.Combine("wwwroot", est.PreparedSignaturePath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var storageName = $"{Guid.NewGuid():N}_prepared_signature.png";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        est.PreparedSignaturePath = $"/uploads/estimates/{est.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Prepared signature uploaded for Estimate {EstimateId}", request.EstimateId);
        return Result.Success();
    }
}

// ── Upload Approver Signature ──
public record UploadApproverSignatureCommand : IRequest<Result>
{
    public Guid EstimateId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
}

public class UploadApproverSignatureHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadApproverSignatureHandler> logger)
    : IRequestHandler<UploadApproverSignatureCommand, Result>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase) { "image/png", "image/jpeg", "image/svg+xml" };
    private const long MaxSize = 2 * 1024 * 1024; // 2 MB

    public async Task<Result> Handle(UploadApproverSignatureCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxSize) return Result.Failure("Signature file exceeds 2 MB limit");
        if (!AllowedTypes.Contains(request.ContentType)) return Result.Failure("Signature must be PNG, JPEG, or SVG");

        var est = await db.Estimates.Include(e => e.Proposal).FirstOrDefaultAsync(e => e.Id == request.EstimateId, ct);
        if (est is null) return Result.NotFound("Estimate not found");
        if (est.SentToId != user.UserId) return Result.Forbidden("Only the designated approver can upload signature");

        var folder = Path.Combine("wwwroot", "uploads", "estimates", est.ProposalId.ToString());
        Directory.CreateDirectory(folder);

        if (!string.IsNullOrEmpty(est.ApproverSignaturePath))
        {
            var oldFile = Path.Combine("wwwroot", est.ApproverSignaturePath.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var storageName = $"{Guid.NewGuid():N}_approver_signature.png";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        est.ApproverSignaturePath = $"/uploads/estimates/{est.ProposalId}/{storageName}";
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Approver signature uploaded for Estimate {EstimateId}", request.EstimateId);
        return Result.Success();
    }
}
