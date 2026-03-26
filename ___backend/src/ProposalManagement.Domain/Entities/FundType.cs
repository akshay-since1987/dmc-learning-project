namespace ProposalManagement.Domain.Entities;

public class FundType
{
    public Guid Id { get; set; }
    public string Name_En { get; set; } = string.Empty;
    public string Name_Alt { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsMnp { get; set; }
    public bool IsState { get; set; }
    public bool IsCentral { get; set; }
    public bool IsDpdc { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Proposal> Proposals { get; set; } = [];
}
