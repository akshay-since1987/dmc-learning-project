using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OtpService> _logger;
    private readonly bool _simulateOtp;
    private readonly string _defaultOtp;
    private readonly string? _smsApiUrl;
    private readonly string? _smsApiKey;
    private readonly string? _smsApiMethod;

    public OtpService(HttpClient httpClient, ILogger<OtpService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _simulateOtp = configuration.GetValue<bool>("Otp:SimulateOtp");
        _defaultOtp = configuration.GetValue<string>("Otp:DefaultOtp") ?? "123456";
        _smsApiUrl = configuration["SmsGateway:ApiUrl"];
        _smsApiKey = configuration["SmsGateway:ApiKey"];
        _smsApiMethod = configuration["SmsGateway:HttpMethod"] ?? "POST";
    }

    public string GenerateOtp()
    {
        if (_simulateOtp)
            return _defaultOtp;

        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    public string HashOtp(string otp)
    {
        return BCrypt.Net.BCrypt.HashPassword(otp);
    }

    public bool VerifyOtp(string otp, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(otp, hash);
    }

    public async Task SendOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default)
    {
        var maskedMobile = MaskMobile(mobileNumber);

        if (_simulateOtp || string.IsNullOrWhiteSpace(_smsApiUrl))
        {
            _logger.LogInformation("OTP simulated for {MaskedMobile}", maskedMobile);
            return;
        }

        try
        {
            // Build the URL — replace {mobile} and {otp} placeholders in the configured URL
            var url = _smsApiUrl
                .Replace("{mobile}", Uri.EscapeDataString(mobileNumber))
                .Replace("{otp}", Uri.EscapeDataString(otp));

            HttpResponseMessage response;

            if (string.Equals(_smsApiMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                // GET-based SMS APIs (common in Indian SMS gateways) — OTP and mobile in query string
                response = await _httpClient.GetAsync(url, cancellationToken);
            }
            else
            {
                // POST-based SMS APIs — send OTP and mobile in JSON body
                var payload = new { mobile = mobileNumber, otp };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8, "application/json");

                if (!string.IsNullOrWhiteSpace(_smsApiKey))
                    content.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_smsApiKey}");

                response = await _httpClient.PostAsync(url, content, cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SMS API failed for {MaskedMobile}: {StatusCode} {Body}",
                    maskedMobile, (int)response.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("OTP sent via SMS to {MaskedMobile}", maskedMobile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP via SMS to {MaskedMobile}", maskedMobile);
            // Don't throw — OTP is already saved in DB; user can retry
        }
    }

    private static string MaskMobile(string mobile)
    {
        if (string.IsNullOrEmpty(mobile) || mobile.Length < 3)
            return "***";
        return $"{mobile[0]}***{mobile[^2..]}";
    }
}
