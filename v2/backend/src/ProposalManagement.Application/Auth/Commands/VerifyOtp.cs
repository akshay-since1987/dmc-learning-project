using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record VerifyOtpCommand(string MobileNumber, string Otp) : IRequest<Result<VerifyOtpResponse>>;

public record VerifyOtpResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string FullName,
    string Role,
    Guid PalikaId);

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, Result<VerifyOtpResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<VerifyOtpHandler> _logger;

    public VerifyOtpHandler(IAppDbContext db, IJwtTokenService jwt, ILogger<VerifyOtpHandler> logger)
    {
        _db = db;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<Result<VerifyOtpResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var otpRequest = await _db.OtpRequests
            .Where(o => o.MobileNumber == request.MobileNumber && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otpRequest is null)
            return Result<VerifyOtpResponse>.Failure("OTP expired or not found");

        if (otpRequest.AttemptCount >= 3)
            return Result<VerifyOtpResponse>.Failure("Maximum OTP attempts exceeded");

        otpRequest.AttemptCount++;

        if (!BCrypt.Net.BCrypt.Verify(request.Otp, otpRequest.OtpHash))
        {
            await _db.SaveChangesAsync(cancellationToken);
            return Result<VerifyOtpResponse>.Failure("Invalid OTP");
        }

        otpRequest.IsUsed = true;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (user is null)
            return Result<VerifyOtpResponse>.Failure("User not found", 404);

        var accessToken = _jwt.GenerateAccessToken(user.Id, user.Role, user.PalikaId);
        var refreshTokenStr = _jwt.GenerateRefreshToken();

        var refreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Result<VerifyOtpResponse>.Success(new VerifyOtpResponse(
            accessToken, refreshTokenStr, user.Id, user.FullName_En, user.Role, user.PalikaId));
    }
}
