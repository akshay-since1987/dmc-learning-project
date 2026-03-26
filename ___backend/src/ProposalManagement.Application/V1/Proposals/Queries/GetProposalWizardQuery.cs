using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.V1.Proposals.DTOs;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.V1.Proposals.Queries;

public record GetProposalWizardQuery(Guid Id) : IRequest<Result<ProposalWizardDto>>;

public class GetProposalWizardQueryHandler : IRequestHandler<GetProposalWizardQuery, Result<ProposalWizardDto>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly ICurrentUser _currentUser;

    public GetProposalWizardQueryHandler(IRepository<Proposal> repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<ProposalWizardDto>> Handle(GetProposalWizardQuery request, CancellationToken ct)
    {
        var role = _currentUser.Role;

        var baseQuery = role == UserRole.Lotus
            ? _repo.QueryIgnoreFilters().AsNoTracking()
            : _repo.Query().AsNoTracking();

        var dto = await baseQuery
            .Where(p => p.Id == request.Id)
            .Select(p => new
            {
                Proposal = new ProposalWizardDto
                {
                    Id = p.Id,
                    ProposalNumber = p.ProposalNumber,
                    CurrentStage = p.CurrentStage.ToString(),
                    CompletedStep = p.CompletedStep,
                    PushBackCount = p.PushBackCount,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,

                    Date = p.Date,
                    DepartmentId = p.DepartmentId,
                    DepartmentName_En = p.Department.Name_En,
                    SubmittedById = p.SubmittedById,
                    SubmittedByName_En = p.SubmittedBy.FullName_En,
                    SubmitterDesignationId = p.SubmitterDesignationId,
                    DesignationName_En = p.SubmitterDesignation.Name_En,
                    Subject_En = p.Subject_En,
                    Subject_Alt = p.Subject_Alt,
                    Reason_En = p.Reason_En,
                    Reason_Alt = p.Reason_Alt,
                    BriefInfo_En = p.BriefInfo_En,
                    BriefInfo_Alt = p.BriefInfo_Alt,
                    FundTypeId = p.FundTypeId,
                    FundTypeName_En = p.FundType.Name_En,
                    FundOwner = p.FundOwner,
                    FundYear = p.FundYear,
                    AccountHeadId = p.AccountHeadId,
                    AccountHeadName_En = p.AccountHead != null ? p.AccountHead.Name_En : null,
                    WardId = p.WardId,
                    WardName_En = p.Ward != null ? p.Ward.Name_En : null,
                    EstimatedCost = p.EstimatedCost,
                    ReferenceNumber = p.ReferenceNumber,

                    SiteInspectionDone = p.SiteInspectionDone,

                    TechnicalApprovalDate = p.TechnicalApprovalDate,
                    TechnicalApprovalCost = p.TechnicalApprovalCost,
                    TechnicalApprovalNumber = p.TechnicalApprovalNumber,
                    CompetentAuthorityTADone = p.CompetentAuthorityTADone,

                    ProcurementMethodId = p.ProcurementMethodId,
                    ProcurementMethodName_En = p.ProcurementMethod != null ? p.ProcurementMethod.Name_En : null,
                    PublicationDays = p.PublicationDays,
                    TenderPeriodVerified = p.TenderPeriodVerified,

                    AccountingOfficerId = p.AccountingOfficerId,
                    AccountingOfficerName_En = p.AccountingOfficer != null ? p.AccountingOfficer.FullName_En : null,
                    AccountingOfficerName_Alt = p.AccountingOfficer != null ? p.AccountingOfficer.FullName_Alt : null,
                    AccountingOfficerMobile = p.AccountingOfficer != null ? p.AccountingOfficer.MobileNumber : null,
                    HomeId = p.HomeId,
                    AccountingNumber = p.AccountingNumber,
                    HasPreviousExpenditure = p.HasPreviousExpenditure,
                    PreviousExpenditureAmount = p.PreviousExpenditureAmount,
                    ApprovedBudget = p.ApprovedBudget,
                    PreviousExpenditure = p.PreviousExpenditure,
                    ProposedWorkCost = p.ProposedWorkCost,
                    RemainingBalance = p.RemainingBalance,
                    BalanceAmount = p.BalanceAmount,
                    AccountantWillingToProcess = p.AccountantWillingToProcess,

                    WorkPlaceWithinPalika = p.WorkPlaceWithinPalika,
                    SiteOwnershipVerified = p.SiteOwnershipVerified,
                    NocObtained = p.NocObtained,
                    LegalSurveyDone = p.LegalSurveyDone,
                    CourtCasePending = p.CourtCasePending,
                    CourtCaseDetails_En = p.CourtCaseDetails_En,
                    CourtCaseDetails_Alt = p.CourtCaseDetails_Alt,

                    DuplicateFundCheckDone = p.DuplicateFundCheckDone,
                    SameWorkProposedInOtherFund = p.SameWorkProposedInOtherFund,
                    VendorTenureCompleted = p.VendorTenureCompleted,
                    LegalObstacleExists = p.LegalObstacleExists,
                    AuditObjectionExists = p.AuditObjectionExists,
                    AuditObjectionDetails_En = p.AuditObjectionDetails_En,
                    AuditObjectionDetails_Alt = p.AuditObjectionDetails_Alt,
                    OtherWorkInProgress = p.OtherWorkInProgress,
                    OtherWorkDetails_En = p.OtherWorkDetails_En,
                    OtherWorkDetails_Alt = p.OtherWorkDetails_Alt,
                    DlpCheckDone = p.DlpCheckDone,
                    OverallComplianceConfirmed = p.OverallComplianceConfirmed,

                    CompetentAuthorityId = p.CompetentAuthorityId,
                    FirstApproverRole = p.FirstApproverRole != null ? p.FirstApproverRole.ToString() : null,
                    SubmitterDeclarationAccepted = p.SubmitterDeclarationAccepted,
                    SubmitterDeclarationText_En = p.SubmitterDeclarationText_En,
                    SubmitterDeclarationText_Alt = p.SubmitterDeclarationText_Alt,
                    SubmitterRemarks_En = p.SubmitterRemarks_En,
                    SubmitterRemarks_Alt = p.SubmitterRemarks_Alt,
                    SubmitterSignedPdfPath = p.Signatures
                        .Where(s => s.SignedById == p.SubmittedById)
                        .OrderByDescending(s => s.CreatedAt)
                        .Select(s => s.GeneratedPdfPath)
                        .FirstOrDefault(),
                    LatestSignedPdfPath = p.Signatures
                        .OrderByDescending(s => s.CreatedAt)
                        .Select(s => s.GeneratedPdfPath)
                        .FirstOrDefault(),

                    Documents = p.Documents
                        .Where(d => !d.IsDeleted)
                        .Select(d => new ProposalDocumentDto(
                            d.Id, d.DocumentType.ToString(), d.FileName,
                            d.FileSize, d.ContentType, d.CreatedAt))
                        .ToList()
                },
                p.SubmittedById,
                p.CurrentStage
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return Result<ProposalWizardDto>.NotFound("Proposal not found");

        // Access control
        var isProposer = dto.SubmittedById == _currentUser.UserId;
        var isGlobalReader = role is UserRole.Commissioner or UserRole.Auditor or UserRole.Lotus;
        var isStageHandler = IsCurrentStageHandler(role, dto.CurrentStage);

        if (!isProposer && !isGlobalReader && !isStageHandler)
            return Result<ProposalWizardDto>.Forbidden("Access denied");

        return Result<ProposalWizardDto>.Success(dto.Proposal);
    }

    private static bool IsCurrentStageHandler(UserRole role, ProposalStage stage) => (role, stage) switch
    {
        (UserRole.CityEngineer, ProposalStage.AtCityEngineer) => true,
        (UserRole.ADO, ProposalStage.AtADO) => true,
        (UserRole.ChiefAccountant, ProposalStage.AtChiefAccountant) => true,
        (UserRole.DeputyCommissioner, ProposalStage.AtDeputyCommissioner) => true,
        (UserRole.Commissioner, ProposalStage.AtCommissioner) => true,
        _ => false
    };
}
