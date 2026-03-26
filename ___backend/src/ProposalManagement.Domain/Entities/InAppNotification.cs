namespace ProposalManagement.Domain.Entities;

public class InAppNotification
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public string Title_En { get; set; } = string.Empty;
    public string Title_Alt { get; set; } = string.Empty;
    public string Message_En { get; set; } = string.Empty;
    public string Message_Alt { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
