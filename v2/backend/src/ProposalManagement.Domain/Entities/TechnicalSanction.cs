using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class TechnicalSanction : BaseEntity
{
    public Guid ProposalId { get; set; }
    public string? TsNumber { get; set; }
    public DateTime? TsDate { get; set; }
    public decimal? TsAmount { get; set; }
    public string? Description_En { get; set; }
    public string? Description_Mr { get; set; }

    // Documents
    public string? TsPdfPath { get; set; }
    public string? OutsideApprovalLetterPath { get; set; }

    // Sanctioned By details
    public string? SanctionedByName { get; set; }
    public string? SanctionedByName_Mr { get; set; }
    public string? SanctionedByDept { get; set; }
    public string? SanctionedByDept_Mr { get; set; }
    public string? SanctionedByDesignation { get; set; }
    public string? SanctionedByDesignation_Mr { get; set; }

    // Prepared by TS role
    public Guid? PreparedById { get; set; }

    // Signed by AE/SE/CityEngineer
    public Guid? SignedById { get; set; }
    public string? SignerSignaturePath { get; set; }
    public DateTime? SignedAt { get; set; }

    public string Status { get; set; } = Enums.TechnicalSanctionStatus.Draft.ToString();

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User? PreparedBy { get; set; }
    public User? SignedBy { get; set; }
}
