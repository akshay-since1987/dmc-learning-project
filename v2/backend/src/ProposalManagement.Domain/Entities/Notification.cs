namespace ProposalManagement.Domain.Entities;

public class Notification
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PalikaId { get; set; }
    public Guid? ProposalId { get; set; }
    public string Type { get; set; } = default!;
    public string Title_En { get; set; } = default!;
    public string? Title_Mr { get; set; }
    public string Message_En { get; set; } = default!;
    public string? Message_Mr { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
    public Palika Palika { get; set; } = default!;
    public Proposal? Proposal { get; set; }
}
