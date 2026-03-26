namespace ProposalManagement.Domain.Entities;

public class CorporationSettings
{
    public int Id { get; set; }
    public string CorporationName_En { get; set; } = string.Empty;
    public string CorporationName_Alt { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = "en";
    public string AlternateLanguage { get; set; } = "mr";
    public string AlternateLanguageLabel { get; set; } = "मराठी";
    public string DefaultDisplayLanguage { get; set; } = "en";
    public bool AutoTranslateEnabled { get; set; } = true;
    public string? LogoUrl { get; set; }
    public string SmsGatewayProvider { get; set; } = string.Empty;
    public string SmsGatewayApiKey { get; set; } = string.Empty;
    public int OtpExpiryMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 3;
    public int LotusSessionTimeoutMinutes { get; set; } = 15;
    public DateTime UpdatedAt { get; set; }
}
