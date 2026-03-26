using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Auth.Commands;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Result>
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<OtpRequest> _otpRepo;
    private readonly IOtpService _otpService;
    private readonly IRepository<CorporationSettings> _settingsRepo;
    private readonly IAuditService _auditService;
    private readonly ILogger<SendOtpCommandHandler> _logger;

    public SendOtpCommandHandler(
        IRepository<User> userRepo,
        IRepository<OtpRequest> otpRepo,
        IOtpService otpService,
        IRepository<CorporationSettings> settingsRepo,
        IAuditService auditService,
        ILogger<SendOtpCommandHandler> logger)
    {
        _userRepo = userRepo;
        _otpRepo = otpRepo;
        _otpService = otpService;
        _settingsRepo = settingsRepo;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.Query()
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (user is null || !user.IsActive)
            return Result.Failure("User not found or inactive", 404);

        var settings = await _settingsRepo.Query().FirstOrDefaultAsync(cancellationToken);
        var maxAttempts = settings?.OtpMaxAttempts ?? 3;
        var expiryMinutes = settings?.OtpExpiryMinutes ?? 5;

        // Check rate limiting: count recent OTPs
        var recentOtps = await _otpRepo.Query()
            .CountAsync(o => o.MobileNumber == request.MobileNumber
                && o.CreatedAt > DateTime.UtcNow.AddMinutes(-15), cancellationToken);

        if (recentOtps >= maxAttempts)
            return Result.Failure("Too many OTP requests. Please try again later.", 429);

        var otp = _otpService.GenerateOtp();
        var otpHash = _otpService.HashOtp(otp);

        var otpRequest = new OtpRequest
        {
            MobileNumber = request.MobileNumber,
            OtpHash = otpHash,
            Purpose = OtpPurpose.Login,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepo.AddAsync(otpRequest, cancellationToken);
        await _otpService.SendOtpAsync(request.MobileNumber, otp, cancellationToken);

        return Result.Success();
    }
}
