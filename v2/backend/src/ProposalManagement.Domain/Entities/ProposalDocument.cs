using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class ProposalDocument : BaseEntity
{
    public Guid ProposalId { get; set; }
    public int TabNumber { get; set; }
    public string DocumentType { get; set; } = default!;
    public string? DocName { get; set; }
    public string FileName { get; set; } = default!;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public Guid UploadedById { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User UploadedBy { get; set; } = default!;
}
