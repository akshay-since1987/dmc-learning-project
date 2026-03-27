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
    public string? PreparedByName_Mr { get; init; }
    public string? PreparedSignaturePath { get; init; }
    public string? SentToRole { get; init; }
    public string? SentToName { get; init; }
    public string? SentToName_Mr { get; init; }
    public string? ApprovedByName { get; init; }
    public string? ApprovedByName_Mr { get; init; }
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
            PreparedByName = est.PreparedBy.FullName_En, PreparedByName_Mr = est.PreparedBy.FullName_Mr,
            PreparedSignaturePath = est.PreparedSignaturePath,
            SentToRole = est.SentToRole, SentToName = est.SentTo?.FullName_En, SentToName_Mr = est.SentTo?.FullName_Mr,
            ApprovedByName = est.ApprovedBy?.FullName_En, ApprovedByName_Mr = est.ApprovedBy?.FullName_Mr, ApproverSignaturePath = est.ApproverSignaturePath,
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

public class UploadEstimatePdfHandler(IAppDbContext db, ICurrentUser user, IFileStorageService fileStorage, ILogger<UploadEstimatePdfHandler> logger)
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

        if (!string.IsNullOrEmpty(est.EstimatePdfPath))
            await fileStorage.DeleteAsync(est.EstimatePdfPath, ct);

        var safeFileName = Path.GetFileName(request.FileName);
        est.EstimatePdfPath = await fileStorage.SaveAsync($"estimates/{est.ProposalId}", safeFileName, request.FileContent, ct);
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

public class UploadPreparedSignatureHandler(IAppDbContext db, ICurrentUser user, IFileStorageService fileStorage, ILogger<UploadPreparedSignatureHandler> logger)
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

        if (!string.IsNullOrEmpty(est.PreparedSignaturePath))
            await fileStorage.DeleteAsync(est.PreparedSignaturePath, ct);

        est.PreparedSignaturePath = await fileStorage.SaveAsync($"estimates/{est.ProposalId}", "prepared_signature.png", request.FileContent, ct);
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

public class UploadApproverSignatureHandler(IAppDbContext db, ICurrentUser user, IFileStorageService fileStorage, ILogger<UploadApproverSignatureHandler> logger)
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

        if (!string.IsNullOrEmpty(est.ApproverSignaturePath))
            await fileStorage.DeleteAsync(est.ApproverSignaturePath, ct);

        est.ApproverSignaturePath = await fileStorage.SaveAsync($"estimates/{est.ProposalId}", "approver_signature.png", request.FileContent, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Approver signature uploaded for Estimate {EstimateId}", request.EstimateId);
        return Result.Success();
    }
}

// ── Sign Estimate PDF (stamp signature onto PDF) ──
public record SignEstimatePdfCommand : IRequest<Result<string>>
{
    public Guid EstimateId { get; init; }
    public string SignatureType { get; init; } = default!; // "prepared" or "approver"
    public int PageNumber { get; init; }
    public decimal PositionX { get; init; }
    public decimal PositionY { get; init; }
    public decimal Width { get; init; }
    public decimal Height { get; init; }
    public decimal Rotation { get; init; }
}

public class SignEstimatePdfHandler(
    IAppDbContext db,
    ICurrentUser user,
    IPdfSignatureStampService pdfStampService,
    ILogger<SignEstimatePdfHandler> logger)
    : IRequestHandler<SignEstimatePdfCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SignEstimatePdfCommand request, CancellationToken ct)
    {
        var est = await db.Estimates
            .Include(e => e.Proposal)
            .Include(e => e.PreparedBy)
            .Include(e => e.ApprovedBy)
            .FirstOrDefaultAsync(e => e.Id == request.EstimateId && !e.IsDeleted, ct);

        if (est is null) return Result<string>.NotFound("Estimate not found");

        if (string.IsNullOrEmpty(est.EstimatePdfPath))
            return Result<string>.Failure("No estimate PDF uploaded yet");

        // Determine which signature to use
        string? signaturePath;
        string signerName;
        string signerRole;

        if (request.SignatureType == "prepared")
        {
            signaturePath = est.PreparedSignaturePath;
            signerName = est.PreparedBy?.FullName_En ?? "Preparer";
            signerRole = "Estimate Preparer";
        }
        else if (request.SignatureType == "approver")
        {
            signaturePath = est.ApproverSignaturePath;
            signerName = est.ApprovedBy?.FullName_En ?? "Approver";
            signerRole = est.SentToRole ?? "Approver";
        }
        else
        {
            return Result<string>.Failure("SignatureType must be 'prepared' or 'approver'");
        }

        if (string.IsNullOrEmpty(signaturePath))
            return Result<string>.Failure($"No {request.SignatureType} signature image uploaded");

        var outputFolder = $"uploads/estimates/{est.ProposalId}";
        var outputFileName = $"{Guid.NewGuid():N}_signed.pdf";

        var context = new SignatureStampContext(
            SignerName: signerName,
            SignerRole: signerRole,
            Terms_En: "I hereby confirm the accuracy of this estimate document.",
            Terms_Alt: "मी या अंदाजपत्रक दस्तऐवजाच्या अचूकतेची पुष्टी करतो.",
            Note_En: null,
            Note_Alt: null);

        var signedPdfPath = await pdfStampService.StampSignatureAsync(
            sourcePdfPath: est.EstimatePdfPath,
            signatureImagePath: signaturePath,
            pageNumber: request.PageNumber,
            positionX: request.PositionX,
            positionY: request.PositionY,
            width: request.Width,
            height: request.Height,
            rotation: request.Rotation,
            outputFolder: outputFolder,
            outputFileName: outputFileName,
            context: context,
            cancellationToken: ct);

        // Update estimate to point to signed PDF
        est.EstimatePdfPath = signedPdfPath;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Estimate PDF signed ({SignatureType}) for Estimate {EstimateId}", request.SignatureType, request.EstimateId);
        return Result<string>.Success(signedPdfPath);
    }
}
