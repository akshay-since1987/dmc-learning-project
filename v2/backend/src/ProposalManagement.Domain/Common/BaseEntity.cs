namespace ProposalManagement.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
}
