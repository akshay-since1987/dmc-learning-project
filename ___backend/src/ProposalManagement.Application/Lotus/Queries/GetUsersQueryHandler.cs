using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Lotus.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Lotus.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserListDto>>
{
    private readonly IRepository<User> _repo;

    public GetUsersQueryHandler(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.QueryIgnoreFilters().AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u => u.FullName_En.ToLower().Contains(search)
                || u.FullName_Alt.Contains(search)
                || u.MobileNumber.Contains(search)
                || (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<UserRole>(request.Role, out var role))
        {
            query = query.Where(u => u.Role == role);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.FullName_En)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListDto(
                u.Id, u.FullName_En, u.FullName_Alt, u.MobileNumber, u.Email,
                u.Role.ToString(), u.DepartmentId, u.Department != null ? u.Department.Name_En : null,
                u.DesignationId, u.Designation != null ? u.Designation.Name_En : null,
                u.IsActive, u.SignaturePath))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
