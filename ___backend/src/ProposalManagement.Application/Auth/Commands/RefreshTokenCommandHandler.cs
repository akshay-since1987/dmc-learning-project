using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken> refreshTokenRepo,
        IRepository<User> userRepo,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _userRepo = userRepo;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _refreshTokenRepo.Query()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
            return Result<AuthResponse>.Unauthorized("Invalid or expired refresh token");

        // Revoke old token
        existingToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepo.UpdateAsync(existingToken, cancellationToken);

        var user = existingToken.User;
        if (!user.IsActive)
            return Result<AuthResponse>.Unauthorized("User account is inactive");

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(newRefreshToken, cancellationToken);

        var userDto = new UserDto(
            user.Id, user.FullName_En, user.FullName_Alt,
            user.MobileNumber, user.Email, user.Role.ToString(),
            user.DepartmentId, user.DesignationId, user.SignaturePath);

        return Result<AuthResponse>.Success(new AuthResponse(newAccessToken, newRefreshTokenValue, userDto));
    }
}
