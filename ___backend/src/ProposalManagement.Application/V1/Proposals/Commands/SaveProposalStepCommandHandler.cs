using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.V1.Proposals.Commands;

public class SaveProposalStepCommandHandler : IRequestHandler<SaveProposalStepCommand, Result<Guid>>
{
    private const string SubmitterDeclarationTextEn = "Final declaration by submitter: I have verified the proposed work and, to the best of my knowledge, this proposal does not violate any legal provisions, financial provisions, Government Resolution, Government direction, court judgement, or court direction. If any such violation is later found, I shall be responsible.";
    private const string SubmitterDeclarationTextAlt = "सादर करणारे अधिकारी यांचा अंतिम अभिप्राय - प्रस्तावित कामांबाबत मी तपासणी असून या कामागिरीद्वारे कोणत्याही कायदेधीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत नाही. या कामागिरीद्वारे कोणत्याही कायदेधीर तरतुदी/वित्तीय तरतुदी/शासन निर्णय/शासन निर्देश/न्यायालयीन निर्णय/न्यायालयीन निर्देश यांचे उल्लंघन होत असल्याचे निदर्शनास आल्यास त्यास मी जबाबदार राहील.";

    private readonly IRepository<Proposal> _repo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public SaveProposalStepCommandHandler(
        IRepository<Proposal> repo,
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _repo = repo;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(SaveProposalStepCommand cmd, CancellationToken ct)
    {
        Proposal entity;

        if (cmd.StepNumber == 1 && cmd.ProposalId is null)
        {
            // Auto-fill department from user profile if not in command
            if (!cmd.DepartmentId.HasValue)
            {
                var userDept = await _userRepo.Query()
                    .AsNoTracking()
                    .Where(u => u.Id == _currentUser.UserId)
                    .Select(u => u.DepartmentId)
                    .FirstOrDefaultAsync(ct);
                cmd.DepartmentId = userDept;
            }

            if (!cmd.DepartmentId.HasValue)
                return Result<Guid>.Failure("Department is required. Please update your profile or contact admin.");

            // Create new proposal
            var year = DateTime.UtcNow.Year;
            var count = await _repo.QueryIgnoreFilters()
                .CountAsync(p => p.CreatedAt.Year == year, ct);
            var proposalNumber = $"DMC/{year}/{count + 1:D4}";

            entity = new Proposal
            {
                Id = Guid.NewGuid(),
                ProposalNumber = proposalNumber,
                Date = cmd.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                SubmittedById = _currentUser.UserId,
                DepartmentId = cmd.DepartmentId.Value,
                CurrentStage = ProposalStage.Draft,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ApplyStep1(entity, cmd);
            entity.CompletedStep = 1;

            await _repo.AddAsync(entity, ct);

            await _auditService.LogAsync(
                AuditAction.Create, "Proposal", entity.Id.ToString(),
                $"Proposal {proposalNumber} created (wizard step 1)",
                AuditModule.Proposal, AuditSeverity.Info,
                cancellationToken: ct);

            return Result<Guid>.Success(entity.Id);
        }

        // Update existing proposal for a specific step
        if (cmd.ProposalId is null)
            return Result<Guid>.Failure("ProposalId is required for steps 2-4");

        entity = await _repo.Query()
            .FirstOrDefaultAsync(p => p.Id == cmd.ProposalId.Value, ct);

        if (entity is null)
            return Result<Guid>.NotFound("Proposal not found");

        // Access control — only proposer or Lotus can edit draft
        if (entity.SubmittedById != _currentUser.UserId && _currentUser.Role != UserRole.Lotus)
            return Result<Guid>.Forbidden("Only the proposer can edit this proposal");

        if (entity.CurrentStage != ProposalStage.Draft && entity.CurrentStage != ProposalStage.PushedBack)
            return Result<Guid>.Failure("Proposal can only be edited in Draft or PushedBack stage");

        switch (cmd.StepNumber)
        {
            case 1: ApplyStep1(entity, cmd); break;
            case 2: ApplyStep2(entity, cmd); break; // Technical Sanction + Publishing
            case 3: ApplyStep3(entity, cmd); break; // Accounting Info
            case 4: ApplyStep4(entity, cmd); break; // Work Place + Compliance + Authority
            default: return Result<Guid>.Failure("Invalid step number (1-4)");
        }

        // Update completed step tracking
        if (cmd.StepNumber > entity.CompletedStep)
            entity.CompletedStep = cmd.StepNumber;

        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);

        await _auditService.LogAsync(
            AuditAction.Update, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} step {cmd.StepNumber} saved",
            AuditModule.Proposal, AuditSeverity.Info,
            cancellationToken: ct);

        return Result<Guid>.Success(entity.Id);
    }

    private static void ApplyStep1(Proposal e, SaveProposalStepCommand cmd)
    {
        if (cmd.DepartmentId.HasValue) e.DepartmentId = cmd.DepartmentId.Value;
        if (cmd.SubmitterDesignationId.HasValue) e.SubmitterDesignationId = cmd.SubmitterDesignationId.Value;
        e.Subject_En = cmd.Subject_En!;
        e.Subject_Alt = cmd.Subject_Alt ?? string.Empty;
        e.BriefInfo_En = cmd.BriefInfo_En ?? string.Empty;
        e.BriefInfo_Alt = cmd.BriefInfo_Alt ?? string.Empty;
        e.FundTypeId = cmd.FundTypeId!.Value;
        e.FundOwner = cmd.FundOwner ?? string.Empty;
        e.FundYear = cmd.FundYear!;
        e.WardId = cmd.WardId;
        e.EstimatedCost = cmd.EstimatedCost!.Value;
        e.SiteInspectionDone = cmd.SiteInspectionDone ?? false;
        if (cmd.Date.HasValue) e.Date = cmd.Date.Value;
    }

    // Step 2: Technical Sanction + Publishing
    private static void ApplyStep2(Proposal e, SaveProposalStepCommand cmd)
    {
        // Technical Sanction fields
        if (cmd.TechnicalApprovalDate.HasValue) e.TechnicalApprovalDate = cmd.TechnicalApprovalDate;
        if (cmd.TechnicalApprovalCost.HasValue) e.TechnicalApprovalCost = cmd.TechnicalApprovalCost;
        if (cmd.TechnicalApprovalNumber != null) e.TechnicalApprovalNumber = cmd.TechnicalApprovalNumber;
        if (cmd.CompetentAuthorityTADone.HasValue) e.CompetentAuthorityTADone = cmd.CompetentAuthorityTADone.Value;

        // Publishing fields
        e.ProcurementMethodId = cmd.ProcurementMethodId;
        if (cmd.PublicationDays.HasValue) e.PublicationDays = cmd.PublicationDays;
        if (cmd.TenderPeriodVerified.HasValue) e.TenderPeriodVerified = cmd.TenderPeriodVerified.Value;
    }

    // Step 3: Accounting Info
    private static void ApplyStep3(Proposal e, SaveProposalStepCommand cmd)
    {
        e.AccountingOfficerId = cmd.AccountingOfficerId; // nullable — can clear
        if (cmd.HomeId != null) e.HomeId = cmd.HomeId;
        if (cmd.AccountingNumber != null) e.AccountingNumber = cmd.AccountingNumber;
        if (cmd.HasPreviousExpenditure.HasValue) e.HasPreviousExpenditure = cmd.HasPreviousExpenditure.Value;
        if (cmd.PreviousExpenditureAmount.HasValue) e.PreviousExpenditureAmount = cmd.PreviousExpenditureAmount;
        if (cmd.ApprovedBudget.HasValue) e.ApprovedBudget = cmd.ApprovedBudget.Value;
        if (cmd.PreviousExpenditure.HasValue) e.PreviousExpenditure = cmd.PreviousExpenditure.Value;
        if (cmd.ProposedWorkCost.HasValue) e.ProposedWorkCost = cmd.ProposedWorkCost.Value;
        if (cmd.AccountantWillingToProcess.HasValue) e.AccountantWillingToProcess = cmd.AccountantWillingToProcess.Value;

        // Auto-compute balance
        e.RemainingBalance = e.ApprovedBudget - e.PreviousExpenditure - e.ProposedWorkCost;
        e.BalanceAmount = e.EstimatedCost > 0 ? e.ApprovedBudget - e.EstimatedCost : null;
    }

    // Step 4: Work Place + Compliance + Authority
    private static void ApplyStep4(Proposal e, SaveProposalStepCommand cmd)
    {
        // Work Place fields
        if (cmd.WorkPlaceWithinPalika.HasValue) e.WorkPlaceWithinPalika = cmd.WorkPlaceWithinPalika.Value;
        if (cmd.SiteOwnershipVerified.HasValue) e.SiteOwnershipVerified = cmd.SiteOwnershipVerified.Value;
        if (cmd.NocObtained.HasValue) e.NocObtained = cmd.NocObtained.Value;
        if (cmd.LegalSurveyDone.HasValue) e.LegalSurveyDone = cmd.LegalSurveyDone.Value;
        if (cmd.CourtCasePending.HasValue) e.CourtCasePending = cmd.CourtCasePending.Value;
        if (cmd.CourtCaseDetails_En != null) e.CourtCaseDetails_En = cmd.CourtCaseDetails_En;
        if (cmd.CourtCaseDetails_Alt != null) e.CourtCaseDetails_Alt = cmd.CourtCaseDetails_Alt;

        // Compliance fields
        if (cmd.DuplicateFundCheckDone.HasValue) e.DuplicateFundCheckDone = cmd.DuplicateFundCheckDone.Value;
        if (cmd.SameWorkProposedInOtherFund.HasValue) e.SameWorkProposedInOtherFund = cmd.SameWorkProposedInOtherFund.Value;
        if (cmd.VendorTenureCompleted.HasValue) e.VendorTenureCompleted = cmd.VendorTenureCompleted.Value;
        if (cmd.LegalObstacleExists.HasValue) e.LegalObstacleExists = cmd.LegalObstacleExists.Value;
        if (cmd.AuditObjectionExists.HasValue) e.AuditObjectionExists = cmd.AuditObjectionExists.Value;
        if (cmd.AuditObjectionDetails_En != null) e.AuditObjectionDetails_En = cmd.AuditObjectionDetails_En;
        if (cmd.AuditObjectionDetails_Alt != null) e.AuditObjectionDetails_Alt = cmd.AuditObjectionDetails_Alt;
        if (cmd.OtherWorkInProgress.HasValue) e.OtherWorkInProgress = cmd.OtherWorkInProgress.Value;
        if (cmd.OtherWorkDetails_En != null) e.OtherWorkDetails_En = cmd.OtherWorkDetails_En;
        if (cmd.OtherWorkDetails_Alt != null) e.OtherWorkDetails_Alt = cmd.OtherWorkDetails_Alt;
        if (cmd.DlpCheckDone.HasValue) e.DlpCheckDone = cmd.DlpCheckDone.Value;
        if (cmd.OverallComplianceConfirmed.HasValue) e.OverallComplianceConfirmed = cmd.OverallComplianceConfirmed.Value;

        // Authority fields
        e.CompetentAuthorityId = cmd.CompetentAuthorityId;
        if (cmd.FirstApproverRole != null && Enum.TryParse<UserRole>(cmd.FirstApproverRole, out var role))
            e.FirstApproverRole = role;

        // Submitter declaration + remarks
        if (cmd.SubmitterDeclarationAccepted.HasValue)
            e.SubmitterDeclarationAccepted = cmd.SubmitterDeclarationAccepted.Value;

        e.SubmitterDeclarationText_En = e.SubmitterDeclarationAccepted ? SubmitterDeclarationTextEn : null;
        e.SubmitterDeclarationText_Alt = e.SubmitterDeclarationAccepted ? SubmitterDeclarationTextAlt : null;

        if (cmd.SubmitterRemarks_En != null)
            e.SubmitterRemarks_En = cmd.SubmitterRemarks_En;
        if (cmd.SubmitterRemarks_Alt != null)
            e.SubmitterRemarks_Alt = cmd.SubmitterRemarks_Alt;
    }
}
