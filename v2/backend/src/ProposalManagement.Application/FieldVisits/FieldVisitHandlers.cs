using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.FieldVisits;

// ── DTOs ──
public record FieldVisitDto
{
    public Guid Id { get; init; }
    public int VisitNumber { get; init; }
    public Guid AssignedToId { get; init; }
    public string? AssignedToName { get; init; }
    public string? InspectionByName { get; init; }
    public DateTime? InspectionDate { get; init; }
    public string? SiteConditionName { get; init; }
    public string? ProblemDescription_En { get; init; }
    public string? ProblemDescription_Mr { get; init; }
    public string? Measurements_En { get; init; }
    public string? Measurements_Mr { get; init; }
    public decimal? GpsLatitude { get; init; }
    public decimal? GpsLongitude { get; init; }
    public string? Remark_En { get; init; }
    public string? Recommendation_En { get; init; }
    public string? UploadedPdfPath { get; init; }
    public string? SignaturePath { get; init; }
    public string Status { get; init; } = default!;
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<FieldVisitPhotoDto> Photos { get; init; } = [];
}

public record FieldVisitPhotoDto(Guid Id, string FileName, long FileSize, string StoragePath, string? Caption, DateTime CreatedAt);

// ── Queries ──
public record GetFieldVisitsQuery(Guid ProposalId) : IRequest<Result<List<FieldVisitDto>>>;

public record GetAssignableEngineersQuery(Guid ProposalId) : IRequest<Result<List<AssignableEngineerDto>>>;

public record AssignableEngineerDto(Guid Id, string FullName_En, string? FullName_Mr, string Role, string? DepartmentName);

public class GetFieldVisitsHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<GetFieldVisitsQuery, Result<List<FieldVisitDto>>>
{
    public async Task<Result<List<FieldVisitDto>>> Handle(GetFieldVisitsQuery request, CancellationToken ct)
    {
        var items = await db.FieldVisits
            .Where(fv => fv.ProposalId == request.ProposalId && !fv.IsDeleted)
            .Include(fv => fv.AssignedTo)
            .Include(fv => fv.InspectionBy)
            .Include(fv => fv.SiteCondition)
            .Include(fv => fv.Photos)
            .OrderBy(fv => fv.VisitNumber)
            .Select(fv => new FieldVisitDto
            {
                Id = fv.Id,
                VisitNumber = fv.VisitNumber,
                AssignedToId = fv.AssignedToId,
                AssignedToName = fv.AssignedTo.FullName_En,
                InspectionByName = fv.InspectionBy != null ? fv.InspectionBy.FullName_En : null,
                InspectionDate = fv.InspectionDate,
                SiteConditionName = fv.SiteCondition != null ? fv.SiteCondition.Name_En : null,
                ProblemDescription_En = fv.ProblemDescription_En,
                ProblemDescription_Mr = fv.ProblemDescription_Mr,
                Measurements_En = fv.Measurements_En,
                Measurements_Mr = fv.Measurements_Mr,
                GpsLatitude = fv.GpsLatitude,
                GpsLongitude = fv.GpsLongitude,
                Remark_En = fv.Remark_En,
                Recommendation_En = fv.Recommendation_En,
                UploadedPdfPath = fv.UploadedPdfPath,
                SignaturePath = fv.SignaturePath,
                Status = fv.Status,
                CompletedAt = fv.CompletedAt,
                CreatedAt = fv.CreatedAt,
                Photos = fv.Photos.Select(p => new FieldVisitPhotoDto(p.Id, p.FileName, p.FileSize, p.StoragePath, p.Caption, p.CreatedAt)).ToList()
            })
            .ToListAsync(ct);

        return Result<List<FieldVisitDto>>.Success(items);
    }
}

// ── Get assignable engineers (JE, TS from same palika) ──
public class GetAssignableEngineersHandler(IAppDbContext db, ICurrentUser user) 
    : IRequestHandler<GetAssignableEngineersQuery, Result<List<AssignableEngineerDto>>>
{
    private static readonly HashSet<string> AssignableRoles = new() { "JE", "TS" };

    public async Task<Result<List<AssignableEngineerDto>>> Handle(GetAssignableEngineersQuery request, CancellationToken ct)
    {
        var palikaId = user.PalikaId!.Value;
        
        var engineers = await db.Users
            .Where(u => u.PalikaId == palikaId && !u.IsDeleted && u.IsActive)
            .Where(u => AssignableRoles.Contains(u.Role))
            .Include(u => u.Department)
            .OrderBy(u => u.Role).ThenBy(u => u.FullName_En)
            .Select(u => new AssignableEngineerDto(
                u.Id, u.FullName_En, u.FullName_Mr, u.Role,
                u.Department != null ? u.Department.Name_En : null))
            .ToListAsync(ct);
        
        return Result<List<AssignableEngineerDto>>.Success(engineers);
    }
}

// ── Commands ──
public record AssignFieldVisitCommand(Guid ProposalId, Guid AssignedToId) : IRequest<Result<Guid>>;

public class AssignFieldVisitHandler(IAppDbContext db, ICurrentUser user, ILogger<AssignFieldVisitHandler> logger) 
    : IRequestHandler<AssignFieldVisitCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AssignFieldVisitCommand request, CancellationToken ct)
    {
        var proposal = await db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null) return Result<Guid>.NotFound();

        var lastVisit = await db.FieldVisits
            .Where(fv => fv.ProposalId == request.ProposalId && !fv.IsDeleted)
            .MaxAsync(fv => (int?)fv.VisitNumber, ct) ?? 0;

        var fv = new FieldVisit
        {
            Id = Guid.NewGuid(),
            ProposalId = request.ProposalId,
            VisitNumber = lastVisit + 1,
            AssignedToId = request.AssignedToId,
            AssignedById = user.UserId!.Value,
            Status = nameof(FieldVisitStatus.Assigned),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.FieldVisits.Add(fv);

        if (proposal.CurrentStage == nameof(ProposalStage.Draft) || proposal.CurrentStage == nameof(ProposalStage.PushedBack))
        {
            proposal.CurrentStage = nameof(ProposalStage.FieldVisitPending);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Field visit {VisitNumber} assigned for Proposal {ProposalId}", fv.VisitNumber, request.ProposalId);
        return Result<Guid>.Success(fv.Id);
    }
}

public record UpdateFieldVisitCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid? SiteConditionId { get; init; }
    public string? ProblemDescription_En { get; init; }
    public string? ProblemDescription_Mr { get; init; }
    public string? Measurements_En { get; init; }
    public string? Measurements_Mr { get; init; }
    public decimal? GpsLatitude { get; init; }
    public decimal? GpsLongitude { get; init; }
    public string? Remark_En { get; init; }
    public string? Remark_Mr { get; init; }
    public string? Recommendation_En { get; init; }
    public string? Recommendation_Mr { get; init; }
}

public class UpdateFieldVisitHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<UpdateFieldVisitCommand, Result>
{
    public async Task<Result> Handle(UpdateFieldVisitCommand request, CancellationToken ct)
    {
        var fv = await db.FieldVisits.FindAsync(new object[] { request.Id }, ct);
        if (fv is null) return Result.NotFound();
        if (fv.AssignedToId != user.UserId) return Result.Forbidden("Only assigned user can update");

        fv.SiteConditionId = request.SiteConditionId;
        fv.ProblemDescription_En = request.ProblemDescription_En;
        fv.ProblemDescription_Mr = request.ProblemDescription_Mr;
        fv.Measurements_En = request.Measurements_En;
        fv.Measurements_Mr = request.Measurements_Mr;
        fv.GpsLatitude = request.GpsLatitude;
        fv.GpsLongitude = request.GpsLongitude;
        fv.Remark_En = request.Remark_En;
        fv.Remark_Mr = request.Remark_Mr;
        fv.Recommendation_En = request.Recommendation_En;
        fv.Recommendation_Mr = request.Recommendation_Mr;
        fv.Status = nameof(FieldVisitStatus.InProgress);
        fv.InspectionById = user.UserId;
        fv.InspectionDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public record CompleteFieldVisitCommand(Guid Id) : IRequest<Result>;

public class CompleteFieldVisitHandler(IAppDbContext db, ICurrentUser user, ILogger<CompleteFieldVisitHandler> logger) 
    : IRequestHandler<CompleteFieldVisitCommand, Result>
{
    public async Task<Result> Handle(CompleteFieldVisitCommand request, CancellationToken ct)
    {
        var fv = await db.FieldVisits.FindAsync(new object[] { request.Id }, ct);
        if (fv is null) return Result.NotFound();
        if (fv.AssignedToId != user.UserId) return Result.Forbidden("Only assigned user can complete");

        fv.Status = nameof(FieldVisitStatus.Completed);
        fv.CompletedAt = DateTime.UtcNow;

        var proposal = await db.Proposals.FindAsync(new object[] { fv.ProposalId }, ct);
        if (proposal is not null && proposal.CurrentStage == nameof(ProposalStage.FieldVisitPending))
        {
            proposal.CurrentStage = nameof(ProposalStage.FieldVisitCompleted);
            if (proposal.CompletedTab < 2) proposal.CompletedTab = 2;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Field visit {Id} completed for Proposal {ProposalId}", request.Id, fv.ProposalId);
        return Result.Success();
    }
}

// ── Photo Upload ──
public record UploadFieldVisitPhotoCommand : IRequest<Result<Guid>>
{
    public Guid FieldVisitId { get; init; }
    public string FileName { get; init; } = default!;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = default!;
    public byte[] FileContent { get; init; } = default!;
    public string? Caption { get; init; }
}

public class UploadFieldVisitPhotoHandler(IAppDbContext db, ICurrentUser user, ILogger<UploadFieldVisitPhotoHandler> logger) 
    : IRequestHandler<UploadFieldVisitPhotoCommand, Result<Guid>>
{
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp"
    };
    private const long MaxPhotoSize = 5 * 1024 * 1024; // 5 MB

    public async Task<Result<Guid>> Handle(UploadFieldVisitPhotoCommand request, CancellationToken ct)
    {
        if (request.FileSize > MaxPhotoSize) return Result<Guid>.Failure("Photo size exceeds 5 MB limit");
        if (!AllowedImageTypes.Contains(request.ContentType)) return Result<Guid>.Failure($"File type '{request.ContentType}' not allowed. Use JPEG, PNG, GIF or WebP.");

        var fv = await db.FieldVisits.FindAsync(new object[] { request.FieldVisitId }, ct);
        if (fv is null) return Result<Guid>.NotFound("Field visit not found");
        if (fv.AssignedToId != user.UserId) return Result<Guid>.Forbidden("Only assigned engineer can upload photos");

        var safeFileName = Path.GetFileName(request.FileName);
        var folder = Path.Combine("wwwroot", "uploads", "field-visits", fv.ProposalId.ToString());
        Directory.CreateDirectory(folder);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var storagePath = Path.Combine(folder, storageName);

        await File.WriteAllBytesAsync(storagePath, request.FileContent, ct);

        var photo = new FieldVisitPhoto
        {
            Id = Guid.NewGuid(),
            FieldVisitId = request.FieldVisitId,
            FileName = safeFileName,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            StoragePath = $"/uploads/field-visits/{fv.ProposalId}/{storageName}",
            Caption = request.Caption,
            CreatedAt = DateTime.UtcNow
        };

        db.FieldVisitPhotos.Add(photo);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Photo {FileName} uploaded for FieldVisit {FieldVisitId}", safeFileName, request.FieldVisitId);
        return Result<Guid>.Success(photo.Id);
    }
}

// ── Delete Photo ──
public record DeleteFieldVisitPhotoCommand(Guid PhotoId) : IRequest<Result>;

public class DeleteFieldVisitPhotoHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<DeleteFieldVisitPhotoCommand, Result>
{
    public async Task<Result> Handle(DeleteFieldVisitPhotoCommand request, CancellationToken ct)
    {
        var photo = await db.FieldVisitPhotos
            .Include(p => p.FieldVisit)
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId, ct);
        if (photo is null) return Result.NotFound();
        if (photo.FieldVisit.AssignedToId != user.UserId) return Result.Forbidden("Only assigned engineer can delete photos");

        // Delete physical file
        var filePath = Path.Combine("wwwroot", photo.StoragePath.TrimStart('/'));
        if (File.Exists(filePath)) File.Delete(filePath);

        db.FieldVisitPhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
