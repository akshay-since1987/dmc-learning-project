using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Proposals.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Queries;

public class GetProposalByIdQueryHandler : IRequestHandler<GetProposalByIdQuery, Result<ProposalDetailDto>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public GetProposalByIdQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<ProposalDetailDto>> Handle(GetProposalByIdQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;

        var baseQuery = role == UserRole.Lotus
            ? _repo.QueryIgnoreFilters().AsNoTracking()
            : _repo.Query().AsNoTracking();

        var dto = await baseQuery
            .Where(p => p.Id == request.Id)
            .Select(p => new
            {
                Dto = new ProposalDetailDto(
                    p.Id,
                    p.ProposalNumber,
                    p.Date,
                    p.DepartmentId,
                    p.Department.Name_En,
                    p.Department.Name_Alt,
                    p.SubmittedById,
                    p.SubmittedBy.FullName_En,
                    p.SubmittedBy.FullName_Alt,
                    p.SubmitterDesignationId,
                    p.SubmitterDesignation.Name_En,
                    p.SubmitterDesignation.Name_Alt,
                    p.Subject_En,
                    p.Subject_Alt,
                    p.FundTypeId,
                    p.FundType.Name_En,
                    p.FundOwner,
                    p.FundYear,
                    p.ReferenceNumber,
                    p.WardId,
                    p.Ward != null ? p.Ward.Name_En : null,
                    p.BriefInfo_En,
                    p.BriefInfo_Alt,
                    p.EstimatedCost,
                    p.AccountHeadId,
                    p.AccountHead != null ? p.AccountHead.Name_En : null,
                    p.ApprovedBudget,
                    p.PreviousExpenditure,
                    p.ProposedWorkCost,
                    p.RemainingBalance,
                    p.SiteInspectionDone,
                    p.TechnicalApprovalDate,
                    p.TechnicalApprovalNumber,
                    p.TechnicalApprovalCost,
                    p.CompetentAuthorityTADone,
                    p.ProcurementMethodId,
                    p.ProcurementMethod != null ? p.ProcurementMethod.Name_En : null,
                    p.TenderPublicationPeriodId,
                    p.TenderPeriodVerified,
                    p.SiteOwnershipVerified,
                    p.NocObtained,
                    p.LegalObstacleExists,
                    p.CourtCasePending,
                    p.CourtCaseDetails_En,
                    p.CourtCaseDetails_Alt,
                    p.AuditObjectionExists,
                    p.AuditObjectionDetails_En,
                    p.AuditObjectionDetails_Alt,
                    p.DuplicateFundCheckDone,
                    p.OtherWorkInProgress,
                    p.OtherWorkDetails_En,
                    p.OtherWorkDetails_Alt,
                    p.DlpCheckDone,
                    p.OverallComplianceConfirmed,
                    p.CompetentAuthorityId,
                    p.CurrentStage.ToString(),
                    p.PushBackCount,
                    p.CreatedAt,
                    p.UpdatedAt),
                p.SubmittedById,
                p.CurrentStage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<ProposalDetailDto>.NotFound("Proposal not found");

        // Access control
        var isProposer = dto.SubmittedById == _currentUser.UserId;
        var isGlobalReader = role == UserRole.Commissioner || role == UserRole.Auditor || role == UserRole.Lotus;
        var isStageHandler = IsCurrentStageHandler(role, dto.CurrentStage);

        if (!isProposer && !isGlobalReader && !isStageHandler)
            return Result<ProposalDetailDto>.Forbidden("You do not have access to this proposal");

        return Result<ProposalDetailDto>.Success(dto.Dto);
    }

    private static bool IsCurrentStageHandler(UserRole role, ProposalStage stage)
    {
        return (role, stage) switch
        {
            (UserRole.CityEngineer, ProposalStage.AtCityEngineer) => true,
            (UserRole.ADO, ProposalStage.AtADO) => true,
            (UserRole.ChiefAccountant, ProposalStage.AtChiefAccountant) => true,
            (UserRole.DeputyCommissioner, ProposalStage.AtDeputyCommissioner) => true,
            (UserRole.Commissioner, ProposalStage.AtCommissioner) => true,
            _ => false
        };
    }
}
