using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Auth.Commands;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Auth.Queries;

public record GetMyProfileQuery : IRequest<Result<UserDto>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<UserDto>>
{
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;

    public GetMyProfileQueryHandler(IRepository<User> userRepo, ICurrentUser currentUser)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            return Result<UserDto>.Unauthorized();

        var user = await _userRepo.QueryIgnoreFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user is null || user.IsDeleted || !user.IsActive)
            return Result<UserDto>.NotFound("User not found");

        var dto = new UserDto(
            user.Id,
            user.FullName_En,
            user.FullName_Alt,
            user.MobileNumber,
            user.Email,
            user.Role.ToString(),
            user.DepartmentId,
            user.DesignationId,
            user.SignaturePath);

        return Result<UserDto>.Success(dto);
    }
}
