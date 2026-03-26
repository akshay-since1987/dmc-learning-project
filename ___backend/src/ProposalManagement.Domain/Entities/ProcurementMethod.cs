namespace ProposalManagement.Domain.Entities;

public class ProcurementMethod
{
    public Guid Id { get; set; }
    public string Name_En { get; set; } = string.Empty;
    public string Name_Alt { get; set; } = string.Empty;
    public string Description_En { get; set; } = string.Empty;
    public string Description_Alt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Proposal> Proposals { get; set; } = [];
}
