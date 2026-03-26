namespace ProposalManagement.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }
    public string Name_En { get; set; } = string.Empty;
    public string Name_Alt { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Proposal> Proposals { get; set; } = [];
}
