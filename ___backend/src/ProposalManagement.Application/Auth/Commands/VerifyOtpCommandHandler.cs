using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Auth.Commands;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<AuthResponse>>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<OtpRequest> _otpRepo;
    private readonly IRepository<RefreshToken> _refreshTokenRepo;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IAuditContext _auditContext;
    private readonly ILogger<VerifyOtpCommandHandler> _logger;

    public VerifyOtpCommandHandler(
        IRepository<User> userRepo,
        IRepository<OtpRequest> otpRepo,
        IRepository<RefreshToken> refreshTokenRepo,
        IOtpService otpService,
        ITokenService tokenService,
        IAuditService auditService,
        IAuditContext auditContext,
        ILogger<VerifyOtpCommandHandler> logger)
    {
        _userRepo = userRepo;
        _otpRepo = otpRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _otpService = otpService;
        _tokenService = tokenService;
        _auditService = auditService;
        _auditContext = auditContext;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.Query()
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await _auditService.LogAsync(
                AuditAction.FailedAuth, "User", request.MobileNumber,
                "Login failed: user not found or inactive",
                AuditModule.Auth, AuditSeverity.Warning,
                cancellationToken: cancellationToken);
            return Result<AuthResponse>.Failure("Invalid credentials", 401);
        }

        // Populate audit context so interceptor + explicit logs carry user info
        _auditContext.UserId = user.Id;
        _auditContext.UserName = user.FullName_En;
        _auditContext.UserRole = user.Role.ToString();

        // Lotus requires password + OTP (two-factor)
        if (user.Role == UserRole.Lotus)
        {
            if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(user.PasswordHash))
                return Result<AuthResponse>.Failure("Password is required for admin login", 401);

            if (!_otpService.VerifyOtp(request.Password, user.PasswordHash))
            {
                await _auditService.LogAsync(
                    AuditAction.FailedAuth, "User", user.Id.ToString(),
                    "Login failed: invalid password for Lotus user",
                    AuditModule.Auth, AuditSeverity.Warning,
                    cancellationToken: cancellationToken);
                return Result<AuthResponse>.Failure("Invalid credentials", 401);
            }
        }

        // Find latest unused, non-expired OTP
        var otpRequest = await _otpRepo.Query()
            .Where(o => o.MobileNumber == request.MobileNumber
                && !o.IsUsed
                && o.ExpiresAt > DateTime.UtcNow
                && o.Purpose == OtpPurpose.Login)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otpRequest is null)
        {
            await _auditService.LogAsync(
                AuditAction.FailedAuth, "User", user.Id.ToString(),
                "Login failed: no valid OTP found",
                AuditModule.Auth, AuditSeverity.Warning,
                cancellationToken: cancellationToken);
            return Result<AuthResponse>.Failure("OTP expired or not found", 401);
        }

        otpRequest.AttemptCount++;

        if (!_otpService.VerifyOtp(request.Otp, otpRequest.OtpHash))
        {
            await _otpRepo.UpdateAsync(otpRequest, cancellationToken);
            await _auditService.LogAsync(
                AuditAction.FailedAuth, "User", user.Id.ToString(),
                "Login failed: invalid OTP",
                AuditModule.Auth, AuditSeverity.Warning,
                cancellationToken: cancellationToken);
            return Result<AuthResponse>.Failure("Invalid OTP", 401);
        }

        otpRequest.IsUsed = true;
        await _otpRepo.UpdateAsync(otpRequest, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepo.AddAsync(refreshToken, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Login, "User", user.Id.ToString(),
            $"User logged in successfully",
            AuditModule.Auth, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        var userDto = new UserDto(
            user.Id, user.FullName_En, user.FullName_Alt,
            user.MobileNumber, user.Email, user.Role.ToString(),
            user.DepartmentId, user.DesignationId, user.SignaturePath);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshTokenValue, userDto));
    }
}
