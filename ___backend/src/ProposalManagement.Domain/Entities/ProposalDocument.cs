using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class ProposalDocument
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public Guid UploadedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
