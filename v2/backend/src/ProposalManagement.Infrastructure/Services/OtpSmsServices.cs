using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

/// <summary>Logs OTP to console instead of sending SMS. Used in development.</summary>
public class SimulatedOtpSmsService(ILogger<SimulatedOtpSmsService> logger) : IOtpSmsService
{
    public Task<bool> SendAsync(string mobileNumber, string otp, CancellationToken ct = default)
    {
        // In dev mode the OTP is always the configured default — just log that we would have sent it
        logger.LogInformation("SMS simulated — OTP for mobile {MaskedMobile} would be sent",
            $"{mobileNumber[0]}***{mobileNumber[^2..]}");
        return Task.FromResult(true);
    }
}

/// <summary>Sends OTP via HTTP-based SMS gateway (MSG91, Twilio, generic REST).</summary>
public class HttpSmsGatewayService : IOtpSmsService
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpSmsGatewayService> _logger;

    public HttpSmsGatewayService(HttpClient http, ILogger<HttpSmsGatewayService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string mobileNumber, string otp, CancellationToken ct = default)
    {
        // This implementation supports a generic REST SMS gateway.
        // The BaseAddress and auth header should be configured via HttpClient factory.
        // Palika-specific gateway config is read by the caller and configures the client.
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["mobile"] = mobileNumber,
                ["otp"] = otp,
                ["message"] = $"Your OTP for Proposal Management System is {otp}. Valid for 5 minutes."
            };

            var response = await _http.PostAsJsonAsync("send-otp", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP SMS sent to {MaskedMobile}",
                    $"{mobileNumber[0]}***{mobileNumber[^2..]}");
                return true;
            }

            _logger.LogWarning("SMS gateway returned {StatusCode} for {MaskedMobile}",
                response.StatusCode, $"{mobileNumber[0]}***{mobileNumber[^2..]}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP SMS to {MaskedMobile}",
                $"{mobileNumber[0]}***{mobileNumber[^2..]}");
            return false;
        }
    }
}
