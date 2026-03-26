using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Workflow.Commands;

// ── Submit Proposal (JE sends into approval chain) ──
public record SubmitProposalCommand(Guid ProposalId) : IRequest<Result>;

public class SubmitProposalHandler : IRequestHandler<SubmitProposalCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly ILogger<SubmitProposalHandler> _logger;

    public SubmitProposalHandler(IAppDbContext db, ICurrentUser user, ILogger<SubmitProposalHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<Result> Handle(SubmitProposalCommand request, CancellationToken ct)
    {
        var proposal = await _db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null) return Result.NotFound();

        if (proposal.CreatedById != _user.UserId)
            return Result.Forbidden("Only the creator can submit");

        if (proposal.CurrentStage is not (nameof(ProposalStage.Draft) or nameof(ProposalStage.PushedBack)))
            return Result.Failure("Proposal must be in Draft or PushedBack to submit");

        // Validate: at least one completed field visit with photos is required
        var hasCompletedVisitWithPhotos = await _db.FieldVisits
            .Where(fv => fv.ProposalId == request.ProposalId && fv.Status == "Completed")
            .AnyAsync(fv => fv.Photos.Any(), ct);

        if (!hasCompletedVisitWithPhotos)
            return Result.Failure("At least one completed field visit with uploaded photos is required before submission");

        // First approver in chain is CityEngineer
        var nextOwner = await _db.Users
            .FirstOrDefaultAsync(u => u.Role == nameof(UserRole.CityEngineer) && u.PalikaId == proposal.PalikaId, ct);

        proposal.CurrentStage = nameof(ProposalStage.AtCityEngineer);
        proposal.CurrentOwnerId = nextOwner?.Id;

        // Record approval history
        var user = await _db.Users.FindAsync(new object[] { _user.UserId!.Value }, ct);
        _db.ProposalApprovals.Add(new ProposalApproval
        {
            ProposalId = proposal.Id,
            StageRole = _user.Role!,
            Action = "Submit",
            ActorId = _user.UserId!.Value,
            ActorName_En = user?.FullName_En,
            ActorName_Mr = user?.FullName_Mr,
            DisclaimerText = "Submitted for approval",
            DisclaimerAccepted = true,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Proposal {ProposalNumber} submitted by {UserId}", proposal.ProposalNumber, _user.UserId);
        return Result.Success();
    }
}

// ── Approve at current stage ──
public record ApproveProposalCommand : IRequest<Result>
{
    public Guid ProposalId { get; init; }
    public string? Opinion_En { get; init; }
    public string? Opinion_Mr { get; init; }
    public bool DisclaimerAccepted { get; init; }
}

public class ApproveProposalHandler : IRequestHandler<ApproveProposalCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly ILogger<ApproveProposalHandler> _logger;

    public ApproveProposalHandler(IAppDbContext db, ICurrentUser user, ILogger<ApproveProposalHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<Result> Handle(ApproveProposalCommand request, CancellationToken ct)
    {
        var proposal = await _db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null) return Result.NotFound();

        if (proposal.CurrentOwnerId != _user.UserId)
            return Result.Forbidden("Only the current stage owner can approve");

        if (!request.DisclaimerAccepted)
            return Result.Failure("Disclaimer must be accepted to approve");

        var currentStage = proposal.CurrentStage;
        var (nextStage, nextRole) = GetNextStage(currentStage, proposal);

        if (nextStage is null)
            return Result.Failure($"Cannot approve from stage {currentStage}");

        // Find next owner
        User? nextOwner = null;
        if (nextRole is not null)
        {
            nextOwner = await _db.Users
                .FirstOrDefaultAsync(u => u.Role == nextRole && u.PalikaId == proposal.PalikaId, ct);
        }

        var user = await _db.Users
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _user.UserId, ct);

        // Record approval
        _db.ProposalApprovals.Add(new ProposalApproval
        {
            ProposalId = proposal.Id,
            StageRole = _user.Role!,
            Action = "Approve",
            ActorId = _user.UserId!.Value,
            ActorName_En = user?.FullName_En,
            ActorName_Mr = user?.FullName_Mr,
            ActorDesignation_En = user?.Designation?.Name_En,
            ActorDesignation_Mr = user?.Designation?.Name_Mr,
            Opinion_En = request.Opinion_En,
            Opinion_Mr = request.Opinion_Mr,
            DisclaimerText = $"Approved at stage {currentStage}",
            DisclaimerAccepted = true,
            CreatedAt = DateTime.UtcNow
        });

        proposal.CurrentStage = nextStage;
        proposal.CurrentOwnerId = nextOwner?.Id;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Proposal {ProposalNumber} approved at {Stage} by {UserId}", proposal.ProposalNumber, currentStage, _user.UserId);
        return Result.Success();
    }

    private static (string? nextStage, string? nextRole) GetNextStage(string currentStage, Proposal proposal)
    {
        // Amount-based routing for final stages
        var cost = proposal.Estimate?.EstimatedCost ?? 0;

        return currentStage switch
        {
            nameof(ProposalStage.AtCityEngineer) => (nameof(ProposalStage.AtAccountOfficer), nameof(UserRole.AccountOfficer)),
            nameof(ProposalStage.AtAccountOfficer) => (nameof(ProposalStage.AtDyCommissioner), nameof(UserRole.DyCommissioner)),
            nameof(ProposalStage.AtDyCommissioner) when cost <= 300000 => (nameof(ProposalStage.Approved), null),
            nameof(ProposalStage.AtDyCommissioner) => (nameof(ProposalStage.AtCommissioner), nameof(UserRole.Commissioner)),
            nameof(ProposalStage.AtCommissioner) when cost <= 2400000 => (nameof(ProposalStage.Approved), null),
            nameof(ProposalStage.AtCommissioner) => (nameof(ProposalStage.AtStandingCommittee), nameof(UserRole.StandingCommittee)),
            nameof(ProposalStage.AtStandingCommittee) when cost <= 2500000 => (nameof(ProposalStage.Approved), null),
            nameof(ProposalStage.AtStandingCommittee) => (nameof(ProposalStage.AtCollector), nameof(UserRole.Collector)),
            nameof(ProposalStage.AtCollector) => (nameof(ProposalStage.Approved), null),
            _ => (null, null)
        };
    }
}

// ── Push Back ──
public record PushBackProposalCommand : IRequest<Result>
{
    public Guid ProposalId { get; init; }
    public string PushBackNote_En { get; init; } = default!;
    public string? PushBackNote_Mr { get; init; }
}

public class PushBackProposalHandler : IRequestHandler<PushBackProposalCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly ILogger<PushBackProposalHandler> _logger;

    public PushBackProposalHandler(IAppDbContext db, ICurrentUser user, ILogger<PushBackProposalHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<Result> Handle(PushBackProposalCommand request, CancellationToken ct)
    {
        var proposal = await _db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null) return Result.NotFound();

        if (proposal.CurrentOwnerId != _user.UserId)
            return Result.Forbidden("Only the current stage owner can push back");

        if (string.IsNullOrWhiteSpace(request.PushBackNote_En))
            return Result.Failure("Push back note is mandatory");

        var user = await _db.Users
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _user.UserId, ct);

        // Record push-back
        _db.ProposalApprovals.Add(new ProposalApproval
        {
            ProposalId = proposal.Id,
            StageRole = _user.Role!,
            Action = "PushBack",
            ActorId = _user.UserId!.Value,
            ActorName_En = user?.FullName_En,
            ActorName_Mr = user?.FullName_Mr,
            ActorDesignation_En = user?.Designation?.Name_En,
            ActorDesignation_Mr = user?.Designation?.Name_Mr,
            PushBackNote_En = request.PushBackNote_En,
            PushBackNote_Mr = request.PushBackNote_Mr,
            DisclaimerText = "Pushed back with notes",
            DisclaimerAccepted = true,
            CreatedAt = DateTime.UtcNow
        });

        // Push back always goes to creator JE
        proposal.CurrentStage = nameof(ProposalStage.PushedBack);
        proposal.CurrentOwnerId = proposal.CreatedById;
        proposal.PushBackCount++;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Proposal {ProposalNumber} pushed back by {UserId}", proposal.ProposalNumber, _user.UserId);
        return Result.Success();
    }
}
