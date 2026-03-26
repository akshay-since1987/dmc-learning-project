namespace ProposalManagement.Domain.Entities;

public class OtpRequest
{
    public long Id { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string OtpHash { get; set; } = default!;
    public string Purpose { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
