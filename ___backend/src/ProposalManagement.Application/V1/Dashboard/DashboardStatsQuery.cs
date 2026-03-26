using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.V1.Dashboard;

public record DashboardStatsQuery : IRequest<DashboardStatsDto>;

public record DashboardStatsDto(
    int Total,
    int Draft,
    int Pending,
    int Approved,
    int PushedBack,
    int Cancelled,
    List<StageCountDto> ByStage,
    List<RecentProposalDto> RecentProposals);

public record StageCountDto(string Stage, int Count);

public record RecentProposalDto(
    Guid Id,
    string ProposalNumber,
    string Subject_En,
    string CurrentStage,
    string SubmittedByName_En,
    decimal EstimatedCost,
    DateTime CreatedAt);

public class DashboardStatsQueryHandler : IRequestHandler<DashboardStatsQuery, DashboardStatsDto>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public DashboardStatsQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<DashboardStatsDto> Handle(DashboardStatsQuery request, CancellationToken ct)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        var baseQuery = role == UserRole.Lotus
            ? _repo.QueryIgnoreFilters().AsNoTracking()
            : _repo.Query().AsNoTracking();

        // Scope based on role
        IQueryable<Proposal> scopedQuery = role switch
        {
            UserRole.Submitter => baseQuery.Where(p => p.SubmittedById == userId),
            UserRole.CityEngineer => baseQuery.Where(p =>
                p.SubmittedById == userId || p.CurrentStage == ProposalStage.AtCityEngineer),
            UserRole.ADO => baseQuery.Where(p =>
                p.SubmittedById == userId || p.CurrentStage == ProposalStage.AtADO),
            UserRole.ChiefAccountant => baseQuery.Where(p =>
                p.CurrentStage == ProposalStage.AtChiefAccountant),
            UserRole.DeputyCommissioner => baseQuery.Where(p =>
                p.CurrentStage == ProposalStage.AtDeputyCommissioner),
            _ => baseQuery // Commissioner, Auditor, Lotus see all
        };

        var stageCounts = await scopedQuery
            .GroupBy(p => p.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = stageCounts.Sum(s => s.Count);
        var draft = stageCounts.Where(s => s.Stage == ProposalStage.Draft).Sum(s => s.Count);
        var approved = stageCounts.Where(s => s.Stage == ProposalStage.Approved).Sum(s => s.Count);
        var pushedBack = stageCounts.Where(s => s.Stage == ProposalStage.PushedBack).Sum(s => s.Count);
        var cancelled = stageCounts.Where(s => s.Stage == ProposalStage.Cancelled).Sum(s => s.Count);
        var pending = total - draft - approved - pushedBack - cancelled;

        var byStage = stageCounts
            .Select(s => new StageCountDto(s.Stage.ToString(), s.Count))
            .OrderBy(s => s.Stage)
            .ToList();

        var recent = await scopedQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new RecentProposalDto(
                p.Id,
                p.ProposalNumber,
                p.Subject_En,
                p.CurrentStage.ToString(),
                p.SubmittedBy.FullName_En,
                p.EstimatedCost,
                p.CreatedAt))
            .ToListAsync(ct);

        return new DashboardStatsDto(total, draft, pending, approved, pushedBack, cancelled, byStage, recent);
    }
}
