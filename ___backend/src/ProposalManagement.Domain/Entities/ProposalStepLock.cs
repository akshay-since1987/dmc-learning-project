namespace ProposalManagement.Domain.Entities;

public class ProposalStepLock
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public int StepNumber { get; set; }
    public Guid LockedById { get; set; }
    public DateTime LockedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsReleased { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public User LockedBy { get; set; } = null!;
}
