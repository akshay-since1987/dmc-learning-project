using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Queries;

public class GetAllProposalsQueryHandler : IRequestHandler<GetAllProposalsQuery, PagedResult<ProposalListDto>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public GetAllProposalsQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProposalListDto>> Handle(GetAllProposalsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;

        if (role != UserRole.Commissioner && role != UserRole.Auditor && role != UserRole.Lotus)
            return new PagedResult<ProposalListDto>([], 0, request.PageIndex, request.PageSize);

        var query = role == UserRole.Lotus
            ? _repo.QueryIgnoreFilters().AsNoTracking()
            : _repo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.ProposalNumber.ToLower().Contains(search) ||
                p.Subject_En.ToLower().Contains(search) ||
                p.Subject_Alt.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Stage) && Enum.TryParse<ProposalStage>(request.Stage, out var stage))
        {
            query = query.Where(p => p.CurrentStage == stage);
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
}
