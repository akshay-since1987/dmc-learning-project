using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Commands;

// ── Create Proposal (Tab 1 Save Draft) ──
public record CreateProposalCommand : IRequest<Result<CreateProposalResponse>>
{
    public Guid DepartmentId { get; init; }
    public Guid DeptWorkCategoryId { get; init; }
    public Guid ZoneId { get; init; }
    public Guid PrabhagId { get; init; }
    public string? Area { get; init; }
    public string? Area_Mr { get; init; }
    public string? LocationAddress_En { get; init; }
    public string? LocationAddress_Mr { get; init; }
    public string WorkTitle_En { get; init; } = default!;
    public string? WorkTitle_Mr { get; init; }
    public string WorkDescription_En { get; init; } = default!;
    public string? WorkDescription_Mr { get; init; }
    public Guid? RequestSourceId { get; init; }
    public string? RequestorName { get; init; }
    public string? RequestorName_Mr { get; init; }
    public string? RequestorMobile { get; init; }
    public string? RequestorAddress { get; init; }
    public string? RequestorAddress_Mr { get; init; }
    public string? RequestorDesignation { get; init; }
    public string? RequestorDesignation_Mr { get; init; }
    public string? RequestorOrganisation { get; init; }
    public string? RequestorOrganisation_Mr { get; init; }
    public string Priority { get; init; } = nameof(Domain.Enums.Priority.Medium);
}

public record CreateProposalResponse(Guid Id, string ProposalNumber);

public class CreateProposalHandler : IRequestHandler<CreateProposalCommand, Result<CreateProposalResponse>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly ILogger<CreateProposalHandler> _logger;

    public CreateProposalHandler(IAppDbContext db, ICurrentUser user, ILogger<CreateProposalHandler> logger)
    {
        _db = db;
        _user = user;
        _logger = logger;
    }

    public async Task<Result<CreateProposalResponse>> Handle(CreateProposalCommand request, CancellationToken ct)
    {
        if (_user.Role is not (nameof(UserRole.JE) or nameof(UserRole.Lotus)))
            return Result<CreateProposalResponse>.Forbidden("Only JE or Lotus can create proposals");

        var palikaId = _user.PalikaId!.Value;

        // Generate proposal number: DMC/YYYY/NNNNN
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _db.Proposals
            .Where(p => p.PalikaId == palikaId && p.ProposalNumber.Contains($"/{year}/"))
            .CountAsync(ct);

        var proposalNumber = $"DMC/{year}/{(lastNumber + 1):D5}";

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = proposalNumber,
            PalikaId = palikaId,
            ProposalDate = DateTime.UtcNow.Date,
            DepartmentId = request.DepartmentId,
            DeptWorkCategoryId = request.DeptWorkCategoryId,
            ZoneId = request.ZoneId,
            PrabhagId = request.PrabhagId,
            Area = request.Area,
            Area_Mr = request.Area_Mr,
            LocationAddress_En = request.LocationAddress_En,
            LocationAddress_Mr = request.LocationAddress_Mr,
            WorkTitle_En = request.WorkTitle_En,
            WorkTitle_Mr = request.WorkTitle_Mr,
            WorkDescription_En = request.WorkDescription_En,
            WorkDescription_Mr = request.WorkDescription_Mr,
            RequestSourceId = request.RequestSourceId,
            RequestorName = request.RequestorName,
            RequestorName_Mr = request.RequestorName_Mr,
            RequestorMobile = request.RequestorMobile,
            RequestorAddress = request.RequestorAddress,
            RequestorAddress_Mr = request.RequestorAddress_Mr,
            RequestorDesignation = request.RequestorDesignation,
            RequestorDesignation_Mr = request.RequestorDesignation_Mr,
            RequestorOrganisation = request.RequestorOrganisation,
            RequestorOrganisation_Mr = request.RequestorOrganisation_Mr,
            Priority = request.Priority,
            CreatedById = _user.UserId!.Value,
            CurrentOwnerId = _user.UserId!.Value,
            CurrentStage = nameof(ProposalStage.Draft),
            CompletedTab = 1
        };

        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Proposal {ProposalNumber} created by {UserId}", proposalNumber, _user.UserId);

        return Result<CreateProposalResponse>.Success(new CreateProposalResponse(proposal.Id, proposalNumber));
    }
}

// ── Update Proposal (Tab 1) ──
public record UpdateProposalCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid DepartmentId { get; init; }
    public Guid DeptWorkCategoryId { get; init; }
    public Guid ZoneId { get; init; }
    public Guid PrabhagId { get; init; }
    public string? Area { get; init; }
    public string? Area_Mr { get; init; }
    public string? LocationAddress_En { get; init; }
    public string? LocationAddress_Mr { get; init; }
    public string WorkTitle_En { get; init; } = default!;
    public string? WorkTitle_Mr { get; init; }
    public string WorkDescription_En { get; init; } = default!;
    public string? WorkDescription_Mr { get; init; }
    public Guid? RequestSourceId { get; init; }
    public string? RequestorName { get; init; }
    public string? RequestorName_Mr { get; init; }
    public string? RequestorMobile { get; init; }
    public string? RequestorAddress { get; init; }
    public string? RequestorAddress_Mr { get; init; }
    public string? RequestorDesignation { get; init; }
    public string? RequestorDesignation_Mr { get; init; }
    public string? RequestorOrganisation { get; init; }
    public string? RequestorOrganisation_Mr { get; init; }
    public string Priority { get; init; } = nameof(Domain.Enums.Priority.Medium);
}

public class UpdateProposalHandler : IRequestHandler<UpdateProposalCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public UpdateProposalHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result> Handle(UpdateProposalCommand request, CancellationToken ct)
    {
        var proposal = await _db.Proposals.FindAsync(new object[] { request.Id }, ct);
        if (proposal is null) return Result.NotFound("Proposal not found");

        // Only creator can update, and only in Draft or PushedBack
        if (proposal.CreatedById != _user.UserId)
            return Result.Forbidden("Only the creator can update this proposal");

        if (proposal.CurrentStage is not (nameof(ProposalStage.Draft) or nameof(ProposalStage.PushedBack)))
            return Result.Failure("Proposal can only be updated in Draft or PushedBack stage");

        proposal.DepartmentId = request.DepartmentId;
        proposal.DeptWorkCategoryId = request.DeptWorkCategoryId;
        proposal.ZoneId = request.ZoneId;
        proposal.PrabhagId = request.PrabhagId;
        proposal.Area = request.Area;
        proposal.Area_Mr = request.Area_Mr;
        proposal.LocationAddress_En = request.LocationAddress_En;
        proposal.LocationAddress_Mr = request.LocationAddress_Mr;
        proposal.WorkTitle_En = request.WorkTitle_En;
        proposal.WorkTitle_Mr = request.WorkTitle_Mr;
        proposal.WorkDescription_En = request.WorkDescription_En;
        proposal.WorkDescription_Mr = request.WorkDescription_Mr;
        proposal.RequestSourceId = request.RequestSourceId;
        proposal.RequestorName = request.RequestorName;
        proposal.RequestorName_Mr = request.RequestorName_Mr;
        proposal.RequestorMobile = request.RequestorMobile;
        proposal.RequestorAddress = request.RequestorAddress;
        proposal.RequestorAddress_Mr = request.RequestorAddress_Mr;
        proposal.RequestorDesignation = request.RequestorDesignation;
        proposal.RequestorDesignation_Mr = request.RequestorDesignation_Mr;
        proposal.RequestorOrganisation = request.RequestorOrganisation;
        proposal.RequestorOrganisation_Mr = request.RequestorOrganisation_Mr;
        proposal.Priority = request.Priority;

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete draft proposal ──
public record DeleteProposalCommand(Guid Id) : IRequest<Result>;

public class DeleteProposalHandler : IRequestHandler<DeleteProposalCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public DeleteProposalHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result> Handle(DeleteProposalCommand request, CancellationToken ct)
    {
        var proposal = await _db.Proposals.FindAsync(new object[] { request.Id }, ct);
        if (proposal is null) return Result.NotFound("Proposal not found");

        if (proposal.CreatedById != _user.UserId)
            return Result.Forbidden();

        if (proposal.CurrentStage != nameof(ProposalStage.Draft))
            return Result.Failure("Only draft proposals can be deleted");

        proposal.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
