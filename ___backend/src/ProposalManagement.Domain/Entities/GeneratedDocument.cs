using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class GeneratedDocument
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public DocumentKind DocumentKind { get; set; }
    public string Title_En { get; set; } = string.Empty;
    public string Title_Alt { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public Guid GeneratedById { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public User GeneratedBy { get; set; } = null!;
}
