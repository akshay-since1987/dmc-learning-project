using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class OtpRequest
{
    public long Id { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
