using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.Dtos;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Queries;

// ── Get Proposal by Id ──
public record GetProposalByIdQuery(Guid Id) : IRequest<Result<ProposalDetailDto>>;

public class GetProposalByIdHandler : IRequestHandler<GetProposalByIdQuery, Result<ProposalDetailDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetProposalByIdHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<ProposalDetailDto>> Handle(GetProposalByIdQuery request, CancellationToken ct)
    {
        var p = await _db.Proposals
            .Include(x => x.Department)
            .Include(x => x.DeptWorkCategory)
            .Include(x => x.Zone)
            .Include(x => x.Prabhag)
            .Include(x => x.RequestSource)
            .Include(x => x.CreatedBy)
            .Include(x => x.CurrentOwner)
            .Include(x => x.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (p is null)
            return Result<ProposalDetailDto>.NotFound("Proposal not found");

        // Access control
        var role = _user.Role;
        var userId = _user.UserId;

        bool allowed = role is nameof(UserRole.Lotus) or nameof(UserRole.Commissioner) or nameof(UserRole.Auditor)
            || p.CreatedById == userId
            || p.CurrentOwnerId == userId;

        if (!allowed)
            return Result<ProposalDetailDto>.Forbidden();

        var dto = new ProposalDetailDto
        {
            Id = p.Id,
            ProposalNumber = p.ProposalNumber,
            ProposalDate = p.ProposalDate,
            DepartmentId = p.DepartmentId,
            DepartmentName = p.Department?.Name_En,
            DeptWorkCategoryId = p.DeptWorkCategoryId,
            WorkCategoryName = p.DeptWorkCategory?.Name_En,
            ZoneId = p.ZoneId,
            ZoneName = p.Zone?.Name_En,
            PrabhagId = p.PrabhagId,
            PrabhagName = p.Prabhag?.Name_En,
            Area = p.Area,
            LocationAddress_En = p.LocationAddress_En,
            LocationAddress_Mr = p.LocationAddress_Mr,
            LocationMapPath = p.LocationMapPath,
            WorkTitle_En = p.WorkTitle_En,
            WorkTitle_Mr = p.WorkTitle_Mr,
            WorkDescription_En = p.WorkDescription_En,
            WorkDescription_Mr = p.WorkDescription_Mr,
            RequestSourceId = p.RequestSourceId,
            RequestSourceName = p.RequestSource?.Name_En,
            RequestorName = p.RequestorName,
            RequestorMobile = p.RequestorMobile,
            RequestorAddress = p.RequestorAddress,
            RequestorDesignation = p.RequestorDesignation,
            RequestorOrganisation = p.RequestorOrganisation,
            Priority = p.Priority,
            CurrentStage = p.CurrentStage,
            CurrentOwnerId = p.CurrentOwnerId,
            CurrentOwnerName = p.CurrentOwner?.FullName_En,
            PushBackCount = p.PushBackCount,
            CompletedTab = p.CompletedTab,
            CreatedById = p.CreatedById,
            CreatedByName = p.CreatedBy?.FullName_En,
            CreatedAt = p.CreatedAt,
            Documents = p.Documents.Select(d => new ProposalDocumentDto(
                d.Id, d.TabNumber, d.DocumentType, d.DocName, d.FileName, d.FileSize, d.ContentType, d.CreatedAt
            )).ToList()
        };

        return Result<ProposalDetailDto>.Success(dto);
    }
}

// ── My Proposals (paged) ──
public record GetMyProposalsQuery(int Page = 1, int PageSize = 20, string? Stage = null, string? Search = null)
    : IRequest<Result<PagedResult<ProposalListDto>>>;

public class GetMyProposalsHandler : IRequestHandler<GetMyProposalsQuery, Result<PagedResult<ProposalListDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetMyProposalsHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<PagedResult<ProposalListDto>>> Handle(GetMyProposalsQuery request, CancellationToken ct)
    {
        var query = _db.Proposals.Where(p => p.CreatedById == _user.UserId && p.PalikaId == _user.PalikaId);

        if (!string.IsNullOrWhiteSpace(request.Stage))
            query = query.Where(p => p.CurrentStage == request.Stage);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.ProposalNumber.Contains(request.Search)
                || p.WorkTitle_En.Contains(request.Search)
                || (p.WorkTitle_Mr != null && p.WorkTitle_Mr.Contains(request.Search)));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProposalListDto(
                p.Id, p.ProposalNumber, p.ProposalDate,
                p.WorkTitle_En, p.WorkTitle_Mr,
                p.Department.Name_En, p.DeptWorkCategory.Name_En,
                p.CurrentStage, p.Priority,
                p.CreatedBy.FullName_En, p.CompletedTab, p.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<ProposalListDto>>.Success(
            new PagedResult<ProposalListDto>(items, totalCount, request.Page, request.PageSize));
    }
}

// ── Pending Approvals (for current user's role) ──
public record GetPendingApprovalsQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ProposalListDto>>>;

public class GetPendingApprovalsHandler : IRequestHandler<GetPendingApprovalsQuery, Result<PagedResult<ProposalListDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetPendingApprovalsHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<PagedResult<ProposalListDto>>> Handle(GetPendingApprovalsQuery request, CancellationToken ct)
    {
        var query = _db.Proposals.Where(p => p.CurrentOwnerId == _user.UserId && p.PalikaId == _user.PalikaId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProposalListDto(
                p.Id, p.ProposalNumber, p.ProposalDate,
                p.WorkTitle_En, p.WorkTitle_Mr,
                p.Department.Name_En, p.DeptWorkCategory.Name_En,
                p.CurrentStage, p.Priority,
                p.CreatedBy.FullName_En, p.CompletedTab, p.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<ProposalListDto>>.Success(
            new PagedResult<ProposalListDto>(items, totalCount, request.Page, request.PageSize));
    }
}

// ── All Proposals (Commissioner, Auditor, Lotus) ──
public record GetAllProposalsQuery(int Page = 1, int PageSize = 20, string? Stage = null, string? Search = null, Guid? DepartmentId = null)
    : IRequest<Result<PagedResult<ProposalListDto>>>;

public class GetAllProposalsHandler : IRequestHandler<GetAllProposalsQuery, Result<PagedResult<ProposalListDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetAllProposalsHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<PagedResult<ProposalListDto>>> Handle(GetAllProposalsQuery request, CancellationToken ct)
    {
        var role = _user.Role;
        if (role is not (nameof(UserRole.Commissioner) or nameof(UserRole.Auditor) or nameof(UserRole.Lotus)))
            return Result<PagedResult<ProposalListDto>>.Forbidden();

        var query = _db.Proposals.Where(p => p.PalikaId == _user.PalikaId);

        if (!string.IsNullOrWhiteSpace(request.Stage))
            query = query.Where(p => p.CurrentStage == request.Stage);
        if (request.DepartmentId.HasValue)
            query = query.Where(p => p.DepartmentId == request.DepartmentId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.ProposalNumber.Contains(request.Search)
                || p.WorkTitle_En.Contains(request.Search));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProposalListDto(
                p.Id, p.ProposalNumber, p.ProposalDate,
                p.WorkTitle_En, p.WorkTitle_Mr,
                p.Department.Name_En, p.DeptWorkCategory.Name_En,
                p.CurrentStage, p.Priority,
                p.CreatedBy.FullName_En, p.CompletedTab, p.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<ProposalListDto>>.Success(
            new PagedResult<ProposalListDto>(items, totalCount, request.Page, request.PageSize));
    }
}

// ── Dashboard Stats ──
public record GetProposalStatsQuery : IRequest<Result<ProposalStatsDto>>;

public class GetProposalStatsHandler : IRequestHandler<GetProposalStatsQuery, Result<ProposalStatsDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetProposalStatsHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<ProposalStatsDto>> Handle(GetProposalStatsQuery request, CancellationToken ct)
    {
        var role = _user.Role;
        IQueryable<Domain.Entities.Proposal> query;

        if (role is nameof(UserRole.Commissioner) or nameof(UserRole.Auditor) or nameof(UserRole.Lotus))
            query = _db.Proposals.Where(p => p.PalikaId == _user.PalikaId);
        else if (role is nameof(UserRole.JE))
            query = _db.Proposals.Where(p => p.CreatedById == _user.UserId);
        else
            query = _db.Proposals.Where(p => p.CurrentOwnerId == _user.UserId || p.CreatedById == _user.UserId);

        var stages = await query
            .GroupBy(p => p.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var draft = stages.Where(s => s.Stage == nameof(ProposalStage.Draft)).Sum(s => s.Count);
        var approved = stages.Where(s => s.Stage == nameof(ProposalStage.Approved)).Sum(s => s.Count);
        var pushedBack = stages.Where(s => s.Stage == nameof(ProposalStage.PushedBack)).Sum(s => s.Count);
        var parked = stages.Where(s => s.Stage == nameof(ProposalStage.Parked)).Sum(s => s.Count);
        var total = stages.Sum(s => s.Count);
        var inProgress = total - draft - approved - pushedBack - parked;

        return Result<ProposalStatsDto>.Success(new ProposalStatsDto(draft, inProgress, pushedBack, parked, approved, total));
    }
}

// ── Approval History ──
public record GetApprovalHistoryQuery(Guid ProposalId) : IRequest<Result<List<ApprovalHistoryDto>>>;

public class GetApprovalHistoryHandler : IRequestHandler<GetApprovalHistoryQuery, Result<List<ApprovalHistoryDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetApprovalHistoryHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<List<ApprovalHistoryDto>>> Handle(GetApprovalHistoryQuery request, CancellationToken ct)
    {
        // Verify access
        var proposal = await _db.Proposals.FindAsync(new object[] { request.ProposalId }, ct);
        if (proposal is null)
            return Result<List<ApprovalHistoryDto>>.NotFound();

        var role = _user.Role;
        var userId = _user.UserId;
        bool allowed = role is nameof(UserRole.Lotus) or nameof(UserRole.Commissioner) or nameof(UserRole.Auditor)
            || proposal.CreatedById == userId
            || proposal.CurrentOwnerId == userId;

        if (!allowed)
            return Result<List<ApprovalHistoryDto>>.Forbidden();

        var items = await _db.ProposalApprovals
            .Where(a => a.ProposalId == request.ProposalId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new ApprovalHistoryDto(
                a.Id, a.StageRole, a.Action,
                a.ActorName_En, a.ActorName_Mr,
                a.ActorDesignation_En,
                a.Opinion_En, a.Opinion_Mr,
                a.PushBackNote_En, a.PushBackNote_Mr,
                a.DisclaimerAccepted, a.CreatedAt))
            .ToListAsync(ct);

        return Result<List<ApprovalHistoryDto>>.Success(items);
    }
}
