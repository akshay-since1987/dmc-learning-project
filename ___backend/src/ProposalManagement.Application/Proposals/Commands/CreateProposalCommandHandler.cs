using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Commands;

public class CreateProposalCommandHandler : IRequestHandler<CreateProposalCommand, Result<Guid>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public CreateProposalCommandHandler(
        IRepository<Proposal> repo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _repo = repo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(CreateProposalCommand request, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _repo.QueryIgnoreFilters()
            .CountAsync(p => p.CreatedAt.Year == year, cancellationToken);
        var proposalNumber = $"DMC/{year}/{count + 1:D4}";

        var entity = new Proposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = proposalNumber,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            DepartmentId = request.DepartmentId,
            SubmittedById = _currentUser.UserId,
            SubmitterDesignationId = request.SubmitterDesignationId,
            Subject_En = request.Subject_En,
            Subject_Alt = request.Subject_Alt,
            FundTypeId = request.FundTypeId,
            FundYear = request.FundYear,
            ReferenceNumber = request.ReferenceNumber,
            WardId = request.WardId,
            BriefInfo_En = request.BriefInfo_En,
            BriefInfo_Alt = request.BriefInfo_Alt,
            EstimatedCost = request.EstimatedCost,
            AccountHeadId = request.AccountHeadId,
            ApprovedBudget = request.ApprovedBudget,
            PreviousExpenditure = request.PreviousExpenditure,
            ProposedWorkCost = request.ProposedWorkCost,
            RemainingBalance = request.ApprovedBudget - request.PreviousExpenditure - request.ProposedWorkCost,
            SiteInspectionDone = request.SiteInspectionDone,
            TechnicalApprovalDate = request.TechnicalApprovalDate,
            TechnicalApprovalNumber = request.TechnicalApprovalNumber,
            TechnicalApprovalCost = request.TechnicalApprovalCost,
            CompetentAuthorityTADone = request.CompetentAuthorityTADone,
            ProcurementMethodId = request.ProcurementMethodId,
            TenderPublicationPeriodId = request.TenderPublicationPeriodId,
            TenderPeriodVerified = request.TenderPeriodVerified,
            SiteOwnershipVerified = request.SiteOwnershipVerified,
            NocObtained = request.NocObtained,
            LegalObstacleExists = request.LegalObstacleExists,
            CourtCasePending = request.CourtCasePending,
            CourtCaseDetails_En = request.CourtCaseDetails_En,
            CourtCaseDetails_Alt = request.CourtCaseDetails_Alt,
            AuditObjectionExists = request.AuditObjectionExists,
            AuditObjectionDetails_En = request.AuditObjectionDetails_En,
            AuditObjectionDetails_Alt = request.AuditObjectionDetails_Alt,
            DuplicateFundCheckDone = request.DuplicateFundCheckDone,
            OtherWorkInProgress = request.OtherWorkInProgress,
            OtherWorkDetails_En = request.OtherWorkDetails_En,
            OtherWorkDetails_Alt = request.OtherWorkDetails_Alt,
            DlpCheckDone = request.DlpCheckDone,
            OverallComplianceConfirmed = request.OverallComplianceConfirmed,
            CompetentAuthorityId = request.CompetentAuthorityId,
            CurrentStage = ProposalStage.Draft,
            PushBackCount = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create, "Proposal", entity.Id.ToString(),
            $"Proposal {proposalNumber} created",
            AuditModule.Proposal, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        return Result<Guid>.Success(entity.Id);
    }
}
