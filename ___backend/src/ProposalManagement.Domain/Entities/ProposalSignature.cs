namespace ProposalManagement.Domain.Entities;

public class ProposalSignature
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public long StageHistoryId { get; set; }
    public Guid SignedById { get; set; }
    public int PageNumber { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Rotation { get; set; }
    public string GeneratedPdfPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = null!;
    public ProposalStageHistory StageHistory { get; set; } = null!;
    public User SignedBy { get; set; } = null!;
}
