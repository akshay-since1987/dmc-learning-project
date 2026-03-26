using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string FullName_En { get; set; } = default!;
    public string? FullName_Mr { get; set; }
    public string MobileNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public Guid PalikaId { get; set; }
    public string? SignaturePath { get; set; }

    // Navigation
    public Palika Palika { get; set; } = default!;
    public Department? Department { get; set; }
    public Designation? Designation { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
