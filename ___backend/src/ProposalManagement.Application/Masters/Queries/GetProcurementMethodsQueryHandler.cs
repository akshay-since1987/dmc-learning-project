using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Masters.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Masters.Queries;

public class GetProcurementMethodsQueryHandler : IRequestHandler<GetProcurementMethodsQuery, PagedResult<MasterListItemDto>>
{
    private readonly IRepository<ProcurementMethod> _repo;

    public GetProcurementMethodsQueryHandler(IRepository<ProcurementMethod> repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<MasterListItemDto>> Handle(GetProcurementMethodsQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d => d.Name_En.ToLower().Contains(search)
                || d.Name_Alt.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name_En)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new MasterListItemDto(d.Id, d.Name_En, d.Name_Alt, null, d.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<MasterListItemDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
