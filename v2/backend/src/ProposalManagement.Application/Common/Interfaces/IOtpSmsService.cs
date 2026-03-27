namespace ProposalManagement.Application.Common.Interfaces;

public interface IOtpSmsService
{
    Task<bool> SendAsync(string mobileNumber, string otp, CancellationToken ct = default);
}
