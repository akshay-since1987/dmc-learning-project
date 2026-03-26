using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Auth.Commands;

public record SendOtpCommand(string MobileNumber) : IRequest<Result>;

public class SendOtpHandler : IRequestHandler<SendOtpCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SendOtpHandler> _logger;

    public SendOtpHandler(IAppDbContext db, IConfiguration config, ILogger<SendOtpHandler> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<Result> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (user is null)
            return Result.Failure("User not found", 404);

        // Generate OTP
        var simulateOtp = _config.GetValue<bool>("Otp:SimulateOtp");
        var otp = simulateOtp
            ? _config.GetValue<string>("Otp:DefaultOtp") ?? "123456"
            : Random.Shared.Next(100000, 999999).ToString();

        var otpHash = BCrypt.Net.BCrypt.HashPassword(otp);

        var otpRequest = new Domain.Entities.OtpRequest
        {
            MobileNumber = request.MobileNumber,
            OtpHash = otpHash,
            Purpose = "Login",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.OtpRequests.Add(otpRequest);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("OTP sent to UserId {UserId}", user.Id);

        return Result.Success();
    }
}
