using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Lotus.DTOs;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Lotus.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDetailDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IRepository<User> _repo;

    public GetUserByIdQueryHandler(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repo.QueryIgnoreFilters().AsNoTracking()
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .Where(u => u.Id == request.Id)
            .Select(u => new UserDetailDto(
                u.Id, u.FullName_En, u.FullName_Alt, u.MobileNumber, u.Email,
                u.Role.ToString(), u.DepartmentId, 
                u.Department != null ? u.Department.Name_En : null,
                u.Department != null ? u.Department.Name_Alt : null,
                u.DesignationId,
                u.Designation != null ? u.Designation.Name_En : null,
                u.Designation != null ? u.Designation.Name_Alt : null,
                u.IsActive, u.SignaturePath, u.CreatedAt, u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<UserDetailDto>.Failure("User not found", 404);

        return Result<UserDetailDto>.Success(dto);
    }
}
