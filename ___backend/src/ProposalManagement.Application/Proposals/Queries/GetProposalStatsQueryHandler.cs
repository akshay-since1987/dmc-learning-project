using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Queries;

public class GetProposalStatsQueryHandler : IRequestHandler<GetProposalStatsQuery, ProposalStatsDto>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public GetProposalStatsQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<ProposalStatsDto> Handle(GetProposalStatsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        IQueryable<Proposal> query;

        // Lotus sees everything including soft-deleted
        if (role == UserRole.Lotus)
            query = _repo.QueryIgnoreFilters().AsNoTracking();
        // Commissioner & Auditor see all active proposals
        else if (role == UserRole.Commissioner || role == UserRole.Auditor)
            query = _repo.Query().AsNoTracking();
        // Submitters see only their own
        else if (role == UserRole.Submitter)
            query = _repo.Query().AsNoTracking().Where(p => p.SubmittedById == userId);
        // Approval officers see proposals at their stage + their submitted ones
        else
        {
            var stageForRole = GetStageForRole(role);
            query = _repo.Query().AsNoTracking()
                .Where(p => p.CurrentStage == stageForRole || p.SubmittedById == userId);
        }

        var stages = await query
            .GroupBy(p => p.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var total = stages.Sum(s => s.Count);
        var approved = stages.Where(s => s.Stage == ProposalStage.Approved).Sum(s => s.Count);
        var pushedBack = stages.Where(s => s.Stage == ProposalStage.PushedBack).Sum(s => s.Count);
        var draft = stages.Where(s => s.Stage == ProposalStage.Draft).Sum(s => s.Count);
        var pending = total - approved - pushedBack - draft;

        return new ProposalStatsDto(total, pending, approved, pushedBack, draft);
    }

    private static ProposalStage GetStageForRole(UserRole role) => role switch
    {
        UserRole.CityEngineer => ProposalStage.AtCityEngineer,
        UserRole.ChiefAccountant => ProposalStage.AtChiefAccountant,
        UserRole.DeputyCommissioner => ProposalStage.AtDeputyCommissioner,
        UserRole.Commissioner => ProposalStage.AtCommissioner,
        _ => ProposalStage.Draft
    };
}
