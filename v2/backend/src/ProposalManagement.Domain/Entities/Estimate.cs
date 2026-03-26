using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class Estimate : BaseEntity
{
    public Guid ProposalId { get; set; }
    public string? EstimatePdfPath { get; set; }
    public decimal? EstimatedCost { get; set; }
    public Guid PreparedById { get; set; }
    public string? PreparedSignaturePath { get; set; }

    // Approval by AE/SE/CityEngineer
    public string? SentToRole { get; set; }
    public Guid? SentToId { get; set; }
    public Guid? ApprovedById { get; set; }
    public string? ApproverSignaturePath { get; set; }
    public bool ApproverDisclaimerAccepted { get; set; }
    public string? ApproverOpinion_En { get; set; }
    public string? ApproverOpinion_Mr { get; set; }
    public string Status { get; set; } = Enums.EstimateStatus.Draft.ToString();
    public string? ReturnQueryNote_En { get; set; }
    public string? ReturnQueryNote_Mr { get; set; }

    public DateTime? ApprovedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User PreparedBy { get; set; } = default!;
    public User? SentTo { get; set; }
    public User? ApprovedBy { get; set; }
}
