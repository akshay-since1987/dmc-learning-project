using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Proposals.Commands;

public class SubmitProposalCommandHandler : IRequestHandler<SubmitProposalCommand, Result<SubmitProposalResultDto>>
{
    private readonly IRepository<Proposal> _repo;
    private readonly IRepository<ProposalStageHistory> _historyRepo;
    private readonly IRepository<ProposalSignature> _signatureRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;
    private readonly IPdfSignatureStampService _pdfSignatureStampService;

    public SubmitProposalCommandHandler(
        IRepository<Proposal> repo,
        IRepository<ProposalStageHistory> historyRepo,
        IRepository<ProposalSignature> signatureRepo,
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService auditService,
        IPdfSignatureStampService pdfSignatureStampService)
    {
        _repo = repo;
        _historyRepo = historyRepo;
        _signatureRepo = signatureRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _auditService = auditService;
        _pdfSignatureStampService = pdfSignatureStampService;
    }

    public async Task<Result<SubmitProposalResultDto>> Handle(SubmitProposalCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<SubmitProposalResultDto>.NotFound("Proposal not found");

        if (entity.SubmittedById != _currentUser.UserId)
            return Result<SubmitProposalResultDto>.Forbidden("Only the proposer can submit this proposal");

        if (entity.CurrentStage != ProposalStage.Draft && entity.CurrentStage != ProposalStage.PushedBack)
            return Result<SubmitProposalResultDto>.Failure("Proposal can only be submitted from Draft or PushedBack stage");

        if (!entity.SubmitterDeclarationAccepted)
            return Result<SubmitProposalResultDto>.Failure("Please accept the submitter declaration before requesting approval");

        if (entity.FirstApproverRole != UserRole.CityEngineer && entity.FirstApproverRole != UserRole.ADO)
            return Result<SubmitProposalResultDto>.Failure("Please select the first approver role (City Engineer or ADO) before requesting approval");

        if (request.SignaturePageNumber <= 0
            || request.SignatureWidth <= 0
            || request.SignatureHeight <= 0
            || string.IsNullOrWhiteSpace(request.GeneratedPdfPath))
        {
            return Result<SubmitProposalResultDto>.Failure("Signature placement on generated PDF is required before requesting approval");
        }

        var fromStage = entity.CurrentStage;
        var isResubmit = fromStage == ProposalStage.PushedBack;

        var user = await _userRepo.Query()
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (string.IsNullOrWhiteSpace(user?.SignaturePath))
            return Result<SubmitProposalResultDto>.Failure("Submitter signature image not found in profile");

        var sourceFolder = Path.GetDirectoryName(request.GeneratedPdfPath.Replace('\\', '/'))?.Replace('\\', '/') ?? $"generated/{entity.Id}";
        var signedPdfPath = await _pdfSignatureStampService.StampSignatureAsync(
            request.GeneratedPdfPath,
            user.SignaturePath,
            request.SignaturePageNumber,
            request.SignaturePositionX,
            request.SignaturePositionY,
            request.SignatureWidth,
            request.SignatureHeight,
            request.SignatureRotation,
            sourceFolder,
            $"{entity.ProposalNumber.Replace("/", "-")}_submitter_signed.pdf",
            new SignatureStampContext(
                user?.FullName_En ?? _currentUser.UserName,
                "Submitter",
                entity.SubmitterDeclarationText_En,
                entity.SubmitterDeclarationText_Alt,
                entity.SubmitterRemarks_En,
                entity.SubmitterRemarks_Alt),
            cancellationToken);

        // Route to CE or ADO based on FirstApproverRole (v1 wizard choice), default to CE
        var targetStage = entity.FirstApproverRole == UserRole.ADO
            ? ProposalStage.AtADO
            : ProposalStage.AtCityEngineer;

        entity.CurrentStage = targetStage;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, cancellationToken);

        var history = new ProposalStageHistory
        {
            ProposalId = entity.Id,
            FromStage = fromStage,
            ToStage = targetStage,
            Action = isResubmit ? WorkflowAction.Resubmit : WorkflowAction.Submit,
            ActionById = _currentUser.UserId,
            ActionByName_En = user?.FullName_En ?? _currentUser.UserName,
            ActionByName_Alt = user?.FullName_Alt ?? string.Empty,
            ActionByDesignation_En = user?.Designation?.Name_En ?? string.Empty,
            ActionByDesignation_Alt = user?.Designation?.Name_Alt ?? string.Empty,
            Opinion_En = entity.SubmitterDeclarationText_En,
            Opinion_Alt = entity.SubmitterDeclarationText_Alt,
            Remarks_En = entity.SubmitterRemarks_En,
            Remarks_Alt = entity.SubmitterRemarks_Alt,
            CreatedAt = DateTime.UtcNow
        };

        await _historyRepo.AddAsync(history, cancellationToken);

        var signature = new ProposalSignature
        {
            Id = Guid.NewGuid(),
            ProposalId = entity.Id,
            StageHistoryId = history.Id,
            SignedById = _currentUser.UserId,
            PageNumber = request.SignaturePageNumber,
            PositionX = request.SignaturePositionX,
            PositionY = request.SignaturePositionY,
            Width = request.SignatureWidth,
            Height = request.SignatureHeight,
            Rotation = request.SignatureRotation,
            GeneratedPdfPath = signedPdfPath,
            CreatedAt = DateTime.UtcNow
        };

        await _signatureRepo.AddAsync(signature, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Submit, "Proposal", entity.Id.ToString(),
            $"Proposal {entity.ProposalNumber} {(isResubmit ? "resubmitted" : "submitted")}",
            AuditModule.Proposal, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create, "ProposalSignature", signature.Id.ToString(),
            $"Submitter signature captured for proposal {entity.ProposalNumber} at submission",
            AuditModule.Document, AuditSeverity.Info,
            cancellationToken: cancellationToken);

        return Result<SubmitProposalResultDto>.Success(new SubmitProposalResultDto(history.Id, targetStage.ToString()));
    }
}
