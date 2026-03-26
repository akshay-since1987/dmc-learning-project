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

public record FieldVisitPhotoDto(Guid Id, string FileName, long FileSize, string? Caption, DateTime CreatedAt);

// ── Queries ──
public record GetFieldVisitsQuery(Guid ProposalId) : IRequest<Result<List<FieldVisitDto>>>;

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
                Photos = fv.Photos.Select(p => new FieldVisitPhotoDto(p.Id, p.FileName, p.FileSize, p.Caption, p.CreatedAt)).ToList()
            })
            .ToListAsync(ct);

        return Result<List<FieldVisitDto>>.Success(items);
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
