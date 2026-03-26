using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class Proposal
{
    public Guid Id { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid SubmittedById { get; set; }
    public Guid SubmitterDesignationId { get; set; }
    public string Subject_En { get; set; } = string.Empty;
    public string Subject_Alt { get; set; } = string.Empty;
    public Guid FundTypeId { get; set; }
    public string FundOwner { get; set; } = string.Empty;
    public string FundYear { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid? WardId { get; set; }
    public string BriefInfo_En { get; set; } = string.Empty;
    public string BriefInfo_Alt { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public Guid? AccountHeadId { get; set; }
    public decimal ApprovedBudget { get; set; }
    public decimal PreviousExpenditure { get; set; }
    public decimal ProposedWorkCost { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool SiteInspectionDone { get; set; }
    public DateOnly? TechnicalApprovalDate { get; set; }
    public string? TechnicalApprovalNumber { get; set; }
    public decimal? TechnicalApprovalCost { get; set; }
    public bool CompetentAuthorityTADone { get; set; }
    public Guid? ProcurementMethodId { get; set; }
    public Guid? TenderPublicationPeriodId { get; set; }
    public bool TenderPeriodVerified { get; set; }
    public bool SiteOwnershipVerified { get; set; }
    public bool NocObtained { get; set; }
    public bool LegalObstacleExists { get; set; }
    public bool CourtCasePending { get; set; }
    public string? CourtCaseDetails_En { get; set; }
    public string? CourtCaseDetails_Alt { get; set; }
    public bool AuditObjectionExists { get; set; }
    public string? AuditObjectionDetails_En { get; set; }
    public string? AuditObjectionDetails_Alt { get; set; }
    public bool DuplicateFundCheckDone { get; set; }
    public bool OtherWorkInProgress { get; set; }
    public string? OtherWorkDetails_En { get; set; }
    public string? OtherWorkDetails_Alt { get; set; }
    public bool DlpCheckDone { get; set; }
    public bool OverallComplianceConfirmed { get; set; }
    public Guid? CompetentAuthorityId { get; set; }
    public ProposalStage CurrentStage { get; set; } = ProposalStage.Draft;
    public int PushBackCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── V1 Wizard Fields ───────────────────────────────────
    // Step 1 — Reason (why this proposal is needed)
    public string? Reason_En { get; set; }
    public string? Reason_Alt { get; set; }

    // Step 4 — Publishing
    public int? PublicationDays { get; set; }

    // Step 5 — Accounting
    public string? HomeId { get; set; }
    public string? AccountingNumber { get; set; }
    public bool HasPreviousExpenditure { get; set; }
    public decimal? PreviousExpenditureAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
    public bool AccountantWillingToProcess { get; set; }

    // Step 6 — Work Place
    public bool WorkPlaceWithinPalika { get; set; }
    public bool LegalSurveyDone { get; set; }

    // Step 7 — Duplication & Compliance
    public bool SameWorkProposedInOtherFund { get; set; }
    public bool VendorTenureCompleted { get; set; }

    // Step 8 — Approval routing
    public UserRole? FirstApproverRole { get; set; }

    // Step 4 final declaration before submit
    public bool SubmitterDeclarationAccepted { get; set; }
    public string? SubmitterDeclarationText_En { get; set; }
    public string? SubmitterDeclarationText_Alt { get; set; }
    public string? SubmitterRemarks_En { get; set; }
    public string? SubmitterRemarks_Alt { get; set; }

    // Step 5 — Accounting Officer (FK to User)
    public Guid? AccountingOfficerId { get; set; }

    // Wizard step tracking (0 = not started, 1–8 = highest completed step)
    public int CompletedStep { get; set; }

    // Navigation
    public Department Department { get; set; } = null!;
    public User SubmittedBy { get; set; } = null!;
    public Designation SubmitterDesignation { get; set; } = null!;
    public FundType FundType { get; set; } = null!;
    public Ward? Ward { get; set; }
    public AccountHead? AccountHead { get; set; }
    public User? AccountingOfficer { get; set; }
    public ProcurementMethod? ProcurementMethod { get; set; }
    public TenderPublicationPeriod? TenderPublicationPeriod { get; set; }
    public User? CompetentAuthority { get; set; }
    public ICollection<ProposalDocument> Documents { get; set; } = [];
    public ICollection<ProposalStageHistory> StageHistory { get; set; } = [];
    public ICollection<GeneratedDocument> GeneratedDocuments { get; set; } = [];
    public ICollection<ProposalSignature> Signatures { get; set; } = [];
}
