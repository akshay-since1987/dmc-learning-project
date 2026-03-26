using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.V1.Proposals.Commands;

/// <summary>
/// Single command for saving any wizard step. Only the fields for the given
/// StepNumber are expected; all others are ignored by the handler.
/// </summary>
public class SaveProposalStepCommand : IRequest<Result<Guid>>
{
    /// <summary>Null for new proposal (Step 1 creates it), GUID for existing</summary>
    public Guid? ProposalId { get; set; }
    public int StepNumber { get; set; }

    // ── Step 1 — Basic Info ───────────────────────
    public DateOnly? Date { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SubmitterDesignationId { get; set; }
    public string? Subject_En { get; set; }
    public string? Subject_Alt { get; set; }
    public string? Reason_En { get; set; }
    public string? Reason_Alt { get; set; }
    public string? BriefInfo_En { get; set; }
    public string? BriefInfo_Alt { get; set; }
    public Guid? FundTypeId { get; set; }
    public string? FundOwner { get; set; }
    public string? FundYear { get; set; }
    public Guid? AccountHeadId { get; set; }
    public Guid? WardId { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? ReferenceNumber { get; set; }

    // ── Step 2 — Field Visit ──────────────────────
    public bool? SiteInspectionDone { get; set; }

    // ── Step 3 — Technical Sanction ───────────────
    public DateOnly? TechnicalApprovalDate { get; set; }
    public decimal? TechnicalApprovalCost { get; set; }
    public string? TechnicalApprovalNumber { get; set; }
    public bool? CompetentAuthorityTADone { get; set; }

    // ── Step 4 — Publishing ───────────────────────
    public Guid? ProcurementMethodId { get; set; }
    public int? PublicationDays { get; set; }
    public bool? TenderPeriodVerified { get; set; }

    // ── Step 5 — Accounting ───────────────────────
    public Guid? AccountingOfficerId { get; set; }
    public string? HomeId { get; set; }
    public string? AccountingNumber { get; set; }
    public bool? HasPreviousExpenditure { get; set; }
    public decimal? PreviousExpenditureAmount { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public decimal? PreviousExpenditure { get; set; }
    public decimal? ProposedWorkCost { get; set; }
    public bool? AccountantWillingToProcess { get; set; }

    // ── Step 6 — Work Place ───────────────────────
    public bool? WorkPlaceWithinPalika { get; set; }
    public bool? SiteOwnershipVerified { get; set; }
    public bool? NocObtained { get; set; }
    public bool? LegalSurveyDone { get; set; }
    public bool? CourtCasePending { get; set; }
    public string? CourtCaseDetails_En { get; set; }
    public string? CourtCaseDetails_Alt { get; set; }

    // ── Step 7 — Duplication & Compliance ─────────
    public bool? DuplicateFundCheckDone { get; set; }
    public bool? SameWorkProposedInOtherFund { get; set; }
    public bool? VendorTenureCompleted { get; set; }
    public bool? LegalObstacleExists { get; set; }
    public bool? AuditObjectionExists { get; set; }
    public string? AuditObjectionDetails_En { get; set; }
    public string? AuditObjectionDetails_Alt { get; set; }
    public bool? OtherWorkInProgress { get; set; }
    public string? OtherWorkDetails_En { get; set; }
    public string? OtherWorkDetails_Alt { get; set; }
    public bool? DlpCheckDone { get; set; }
    public bool? OverallComplianceConfirmed { get; set; }

    // ── Step 8 — Approval Authority ───────────────
    public Guid? CompetentAuthorityId { get; set; }
    public string? FirstApproverRole { get; set; }

    // ── Step 4 Final Declaration ──────────────────
    public bool? SubmitterDeclarationAccepted { get; set; }
    public string? SubmitterRemarks_En { get; set; }
    public string? SubmitterRemarks_Alt { get; set; }
}

public class SaveProposalStepCommandValidator : AbstractValidator<SaveProposalStepCommand>
{
    public SaveProposalStepCommandValidator()
    {
        RuleFor(x => x.StepNumber).InclusiveBetween(1, 4);

        // Step 1 validation — required when StepNumber == 1
        When(x => x.StepNumber == 1, () =>
        {
            RuleFor(x => x.SubmitterDesignationId).NotEmpty();
            RuleFor(x => x.Subject_En).NotEmpty().MaximumLength(500);
            RuleFor(x => x.BriefInfo_En).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.FundTypeId).NotEmpty();
            RuleFor(x => x.FundYear).NotEmpty().MaximumLength(20);
            RuleFor(x => x.EstimatedCost).NotNull().GreaterThan(0);
        });

        // Step 2 — Technical Sanction + Publishing
        When(x => x.StepNumber == 2, () =>
        {
            RuleFor(x => x.ProposalId).NotEmpty();
        });

        // Step 3 — Accounting Info
        When(x => x.StepNumber == 3, () =>
        {
            RuleFor(x => x.ProposalId).NotEmpty();
        });

        // Step 4 — Work Place + Compliance + Authority
        When(x => x.StepNumber == 4, () =>
        {
            RuleFor(x => x.ProposalId).NotEmpty();
            RuleFor(x => x.SubmitterRemarks_En).MaximumLength(2000);
            RuleFor(x => x.SubmitterRemarks_Alt).MaximumLength(2000);
        });
    }
}
