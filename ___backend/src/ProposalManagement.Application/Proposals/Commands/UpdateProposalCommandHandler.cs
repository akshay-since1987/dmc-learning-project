using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Commands;

public class UpdateProposalCommandHandler : IRequestHandler<UpdateProposalCommand, Result>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public UpdateProposalCommandHandler(
        IRepository<Proposal> repo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _repo = repo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result> Handle(UpdateProposalCommand request, CancellationToken cancellationToken)
    {
        var isLotus = _currentUser.Role == UserRole.Lotus;

        var entity = isLotus
            ? await _repo.QueryIgnoreFilters().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            : await _repo.Query().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.NotFound("Proposal not found");

        if (!isLotus)
        {
            if (entity.SubmittedById != _currentUser.UserId)
                return Result.Forbidden("Only the proposer can update this proposal");

            if (entity.CurrentStage != ProposalStage.Draft && entity.CurrentStage != ProposalStage.PushedBack)
                return Result.Failure("Proposal can only be updated in Draft or PushedBack stage");
        }

        entity.DepartmentId = request.DepartmentId;
        entity.SubmitterDesignationId = request.SubmitterDesignationId;
        entity.Subject_En = request.Subject_En;
        entity.Subject_Alt = request.Subject_Alt;
        entity.FundTypeId = request.FundTypeId;
        entity.FundYear = request.FundYear;
        entity.ReferenceNumber = request.ReferenceNumber;
        entity.WardId = request.WardId;
        entity.BriefInfo_En = request.BriefInfo_En;
        entity.BriefInfo_Alt = request.BriefInfo_Alt;
        entity.EstimatedCost = request.EstimatedCost;
        entity.AccountHeadId = request.AccountHeadId;
        entity.ApprovedBudget = request.ApprovedBudget;
        entity.PreviousExpenditure = request.PreviousExpenditure;
        entity.ProposedWorkCost = request.ProposedWorkCost;
        entity.RemainingBalance = request.ApprovedBudget - request.PreviousExpenditure - request.ProposedWorkCost;
        entity.SiteInspectionDone = request.SiteInspectionDone;
        entity.TechnicalApprovalDate = request.TechnicalApprovalDate;
        entity.TechnicalApprovalNumber = request.TechnicalApprovalNumber;
        entity.TechnicalApprovalCost = request.TechnicalApprovalCost;
        entity.CompetentAuthorityTADone = request.CompetentAuthorityTADone;
        entity.ProcurementMethodId = request.ProcurementMethodId;
        entity.TenderPublicationPeriodId = request.TenderPublicationPeriodId;
        entity.TenderPeriodVerified = request.TenderPeriodVerified;
        entity.SiteOwnershipVerified = request.SiteOwnershipVerified;
        entity.NocObtained = request.NocObtained;
        entity.LegalObstacleExists = request.LegalObstacleExists;
        entity.CourtCasePending = request.CourtCasePending;
        entity.CourtCaseDetails_En = request.CourtCaseDetails_En;
        entity.CourtCaseDetails_Alt = request.CourtCaseDetails_Alt;
        entity.AuditObjectionExists = request.AuditObjectionExists;
        entity.AuditObjectionDetails_En = request.AuditObjectionDetails_En;
        entity.AuditObjectionDetails_Alt = request.AuditObjectionDetails_Alt;
        entity.DuplicateFundCheckDone = request.DuplicateFundCheckDone;
        entity.OtherWorkInProgress = request.OtherWorkInProgress;
        entity.OtherWorkDetails_En = request.OtherWorkDetails_En;
        entity.OtherWorkDetails_Alt = request.OtherWorkDetails_Alt;
        entity.DlpCheckDone = request.DlpCheckDone;
        entity.OverallComplianceConfirmed = request.OverallComplianceConfirmed;
        entity.CompetentAuthorityId = request.CompetentAuthorityId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Update, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} updated",
            AuditModule.Proposal, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
