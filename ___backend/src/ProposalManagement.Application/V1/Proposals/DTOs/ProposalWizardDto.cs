namespace ProposalManagement.Application.V1.Proposals.DTOs;

/// <summary>Full proposal detail for the v1 wizard — includes all step fields + step tracking</summary>
public record ProposalWizardDto
{
    // Identity
    public Guid Id { get; init; }
    public string ProposalNumber { get; init; } = string.Empty;
    public string CurrentStage { get; init; } = string.Empty;
    public int CompletedStep { get; init; }
    public int PushBackCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    // Step 1 — Basic Info
    public DateOnly Date { get; init; }
    public Guid DepartmentId { get; init; }
    public string? DepartmentName_En { get; init; }
    public Guid SubmittedById { get; init; }
    public string? SubmittedByName_En { get; init; }
    public Guid SubmitterDesignationId { get; init; }
    public string? DesignationName_En { get; init; }
    public string Subject_En { get; init; } = string.Empty;
    public string Subject_Alt { get; init; } = string.Empty;
    public string? Reason_En { get; init; }
    public string? Reason_Alt { get; init; }
    public string BriefInfo_En { get; init; } = string.Empty;
    public string BriefInfo_Alt { get; init; } = string.Empty;
    public Guid FundTypeId { get; init; }
    public string? FundTypeName_En { get; init; }
    public string FundOwner { get; init; } = string.Empty;
    public string FundYear { get; init; } = string.Empty;
    public Guid? AccountHeadId { get; init; }
    public string? AccountHeadName_En { get; init; }
    public Guid? WardId { get; init; }
    public string? WardName_En { get; init; }
    public decimal EstimatedCost { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;

    // Step 2 — Field Visit
    public bool SiteInspectionDone { get; init; }

    // Step 3 — Technical Sanction
    public DateOnly? TechnicalApprovalDate { get; init; }
    public decimal? TechnicalApprovalCost { get; init; }
    public string? TechnicalApprovalNumber { get; init; }
    public bool CompetentAuthorityTADone { get; init; }

    // Step 4 — Publishing
    public Guid? ProcurementMethodId { get; init; }
    public string? ProcurementMethodName_En { get; init; }
    public int? PublicationDays { get; init; }
    public bool TenderPeriodVerified { get; init; }

    // Step 5 — Accounting
    public Guid? AccountingOfficerId { get; init; }
    public string? AccountingOfficerName_En { get; init; }
    public string? AccountingOfficerName_Alt { get; init; }
    public string? AccountingOfficerMobile { get; init; }
    public string? HomeId { get; init; }
    public string? AccountingNumber { get; init; }
    public bool HasPreviousExpenditure { get; init; }
    public decimal? PreviousExpenditureAmount { get; init; }
    public decimal ApprovedBudget { get; init; }
    public decimal PreviousExpenditure { get; init; }
    public decimal ProposedWorkCost { get; init; }
    public decimal RemainingBalance { get; init; }
    public decimal? BalanceAmount { get; init; }
    public bool AccountantWillingToProcess { get; init; }

    // Step 6 — Work Place
    public bool WorkPlaceWithinPalika { get; init; }
    public bool SiteOwnershipVerified { get; init; }
    public bool NocObtained { get; init; }
    public bool LegalSurveyDone { get; init; }
    public bool CourtCasePending { get; init; }
    public string? CourtCaseDetails_En { get; init; }
    public string? CourtCaseDetails_Alt { get; init; }

    // Step 7 — Duplication & Compliance
    public bool DuplicateFundCheckDone { get; init; }
    public bool SameWorkProposedInOtherFund { get; init; }
    public bool VendorTenureCompleted { get; init; }
    public bool LegalObstacleExists { get; init; }
    public bool AuditObjectionExists { get; init; }
    public string? AuditObjectionDetails_En { get; init; }
    public string? AuditObjectionDetails_Alt { get; init; }
    public bool OtherWorkInProgress { get; init; }
    public string? OtherWorkDetails_En { get; init; }
    public string? OtherWorkDetails_Alt { get; init; }
    public bool DlpCheckDone { get; init; }
    public bool OverallComplianceConfirmed { get; init; }

    // Step 8 — Approval Authority
    public Guid? CompetentAuthorityId { get; init; }
    public string? FirstApproverRole { get; init; }

    // Step 4 final declaration
    public bool SubmitterDeclarationAccepted { get; init; }
    public string? SubmitterDeclarationText_En { get; init; }
    public string? SubmitterDeclarationText_Alt { get; init; }
    public string? SubmitterRemarks_En { get; init; }
    public string? SubmitterRemarks_Alt { get; init; }
    public string? SubmitterSignedPdfPath { get; init; }
    public string? LatestSignedPdfPath { get; init; }

    // Documents (attached to proposal)
    public List<ProposalDocumentDto> Documents { get; init; } = [];
}

public record ProposalDocumentDto(
    Guid Id,
    string DocumentType,
    string FileName,
    long FileSize,
    string ContentType,
    DateTime CreatedAt);
