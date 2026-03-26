using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class NotificationLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
}
