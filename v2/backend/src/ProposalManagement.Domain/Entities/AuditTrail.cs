namespace ProposalManagement.Domain.Entities;

public class AuditTrail
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid? PalikaId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string Module { get; set; } = default!;
    public string Severity { get; set; } = default!;
}
