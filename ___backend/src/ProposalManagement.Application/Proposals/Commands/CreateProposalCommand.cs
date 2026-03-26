using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Proposals.Commands;

public record CreateProposalCommand(
    Guid DepartmentId,
    Guid SubmitterDesignationId,
    string Subject_En,
    string Subject_Alt,
    Guid FundTypeId,
    string FundYear,
    string ReferenceNumber,
    Guid? WardId,
    string BriefInfo_En,
    string BriefInfo_Alt,
    decimal EstimatedCost,
    Guid AccountHeadId,
    decimal ApprovedBudget,
    decimal PreviousExpenditure,
    decimal ProposedWorkCost,
    bool SiteInspectionDone,
    DateOnly? TechnicalApprovalDate,
    string? TechnicalApprovalNumber,
    decimal? TechnicalApprovalCost,
    bool CompetentAuthorityTADone,
    Guid? ProcurementMethodId,
    Guid? TenderPublicationPeriodId,
    bool TenderPeriodVerified,
    bool SiteOwnershipVerified,
    bool NocObtained,
    bool LegalObstacleExists,
    bool CourtCasePending,
    string? CourtCaseDetails_En,
    string? CourtCaseDetails_Alt,
    bool AuditObjectionExists,
    string? AuditObjectionDetails_En,
    string? AuditObjectionDetails_Alt,
    bool DuplicateFundCheckDone,
    bool OtherWorkInProgress,
    string? OtherWorkDetails_En,
    string? OtherWorkDetails_Alt,
    bool DlpCheckDone,
    bool OverallComplianceConfirmed,
    Guid? CompetentAuthorityId
) : IRequest<Result<Guid>>;

public class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.SubmitterDesignationId).NotEmpty();
        RuleFor(x => x.Subject_En).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Subject_Alt).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FundTypeId).NotEmpty();
        RuleFor(x => x.FundYear).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BriefInfo_En).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.BriefInfo_Alt).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EstimatedCost).GreaterThan(0);
        RuleFor(x => x.AccountHeadId).NotEmpty();
        RuleFor(x => x.ApprovedBudget).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PreviousExpenditure).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProposedWorkCost).GreaterThan(0);
        RuleFor(x => x.TechnicalApprovalNumber).MaximumLength(100);
        RuleFor(x => x.CourtCaseDetails_En).MaximumLength(2000);
        RuleFor(x => x.CourtCaseDetails_Alt).MaximumLength(2000);
        RuleFor(x => x.AuditObjectionDetails_En).MaximumLength(2000);
        RuleFor(x => x.AuditObjectionDetails_Alt).MaximumLength(2000);
        RuleFor(x => x.OtherWorkDetails_En).MaximumLength(2000);
        RuleFor(x => x.OtherWorkDetails_Alt).MaximumLength(2000);
    }
}
