using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Queries;

public record GetMeQuery : IRequest<Result<MeResponse>>;

public record MeResponse(
    Guid Id,
    string FullName_En,
    string? FullName_Mr,
    string MobileNumber,
    string? Email,
    string Role,
    Guid PalikaId,
    string? DepartmentName,
    string? DesignationName,
    string? SignaturePath);

public class GetMeHandler : IRequestHandler<GetMeQuery, Result<MeResponse>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public GetMeHandler(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<MeResponse>> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (_user.UserId is null) return Result<MeResponse>.Failure("Not authenticated", 401);

        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _user.UserId, ct);

        if (user is null) return Result<MeResponse>.NotFound("User not found");

        return Result<MeResponse>.Success(new MeResponse(
            user.Id, user.FullName_En, user.FullName_Mr, user.MobileNumber, user.Email,
            user.Role, user.PalikaId,
            user.Department?.Name_En, user.Designation?.Name_En,
            user.SignaturePath));
    }
}
