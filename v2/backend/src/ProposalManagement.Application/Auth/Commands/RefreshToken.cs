using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public RefreshTokenHandler(IAppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow, ct);

        if (stored is null)
            return Result<RefreshTokenResponse>.Failure("Invalid or expired refresh token", 401);

        // Revoke old token
        stored.RevokedAt = DateTime.UtcNow;

        // Issue new tokens
        var user = stored.User;
        var accessToken = _jwt.GenerateAccessToken(user.Id, user.Role, user.PalikaId);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(accessToken, newRefreshToken));
    }
}
