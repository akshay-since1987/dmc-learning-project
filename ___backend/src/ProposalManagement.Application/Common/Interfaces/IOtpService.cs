namespace ProposalManagement.Application.Common.Interfaces;

public interface IOtpService
{
    string GenerateOtp();
    string HashOtp(string otp);
    bool VerifyOtp(string otp, string hash);
    Task SendOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default);
}
