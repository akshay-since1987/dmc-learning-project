using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Queries;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, PagedResult<ProposalListDto>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public GetPendingApprovalsQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProposalListDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var targetStage = MapRoleToStage(_currentUser.Role);
        if (targetStage is null)
            return new PagedResult<ProposalListDto>([], 0, request.PageIndex, request.PageSize);

        var query = _repo.Query().AsNoTracking()
            .Where(p => p.CurrentStage == targetStage.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.ProposalNumber.ToLower().Contains(search) ||
                p.Subject_En.ToLower().Contains(search) ||
                p.Subject_Alt.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProposalListDto(
                p.Id,
                p.ProposalNumber,
                p.Date,
                p.Subject_En,
                p.Subject_Alt,
                p.Department.Name_En,
                p.CurrentStage.ToString(),
                p.EstimatedCost,
                p.SubmittedBy.FullName_En,
                p.PushBackCount,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProposalListDto>(items, totalCount, request.PageIndex, request.PageSize);
    }

    private static ProposalStage? MapRoleToStage(UserRole role)
    {
        return role switch
        {
            UserRole.CityEngineer => ProposalStage.AtCityEngineer,
            UserRole.ADO => ProposalStage.AtADO,
            UserRole.ChiefAccountant => ProposalStage.AtChiefAccountant,
            UserRole.DeputyCommissioner => ProposalStage.AtDeputyCommissioner,
            UserRole.Commissioner => ProposalStage.AtCommissioner,
            _ => null
        };
    }
}
