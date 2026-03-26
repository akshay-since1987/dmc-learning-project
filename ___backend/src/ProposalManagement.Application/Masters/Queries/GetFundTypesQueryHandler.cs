using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Queries;

public class GetFundTypesQueryHandler : IRequestHandler<GetFundTypesQuery, PagedResult<FundTypeListItemDto>>
{
    private readonly IRepository<FundType> _repo;

    public GetFundTypesQueryHandler(IRepository<FundType> repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<FundTypeListItemDto>> Handle(GetFundTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d => d.Name_En.ToLower().Contains(search)
                || d.Name_Alt.Contains(search)
                || d.Code.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name_En)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new FundTypeListItemDto(d.Id, d.Name_En, d.Name_Alt, d.Code, d.IsActive,
                d.IsMnp, d.IsState, d.IsCentral, d.IsDpdc))
            .ToListAsync(cancellationToken);

        return new PagedResult<FundTypeListItemDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
