namespace ProposalManagement.Domain.Entities;

public class GeneratedPdf
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string PdfType { get; set; } = default!;
    public int? TabNumber { get; set; }
    public string? StageRole { get; set; }
    public string? Title_En { get; set; }
    public string? Title_Mr { get; set; }
    public string StoragePath { get; set; } = default!;
    public Guid GeneratedById { get; set; }
    public long? FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User GeneratedBy { get; set; } = default!;
}
