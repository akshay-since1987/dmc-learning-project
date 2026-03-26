using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName_En { get; set; } = string.Empty;
    public string FullName_Alt { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public string? SignaturePath { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Department? Department { get; set; }
    public Designation? Designation { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Proposal> SubmittedProposals { get; set; } = [];
}
