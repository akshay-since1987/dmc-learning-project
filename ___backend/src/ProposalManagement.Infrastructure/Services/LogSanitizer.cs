namespace ProposalManagement.Infrastructure.Services;

public static class LogSanitizer
{
    public static string MaskMobile(string mobile)
    {
        if (string.IsNullOrEmpty(mobile) || mobile.Length < 3)
            return "***";
        return $"{mobile[0]}***{mobile[^2..]}";
    }

    public static string RedactToken(string? token) => "***";
}
