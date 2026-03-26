using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.V1.Pdf;

public static class SignatureTermsText
{
    public const string ApprovalTermsEn = "I have reviewed this proposal and agree to the approval terms before signing in my official capacity.";
    public const string ApprovalTermsAlt = "मी हा प्रस्ताव तपासून अधिकृत भूमिकेतून मान्यता अटी मान्य करून स्वाक्षरी करीत आहे.";

    public static string? CombineNote(string? opinion, string? remarks)
    {
        var opinionValue = string.IsNullOrWhiteSpace(opinion) ? null : opinion.Trim();
        var remarksValue = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();

        if (opinionValue is null && remarksValue is null)
            return null;
        if (opinionValue is null)
            return remarksValue;
        if (remarksValue is null)
            return opinionValue;

        return $"Opinion: {opinionValue} | Remarks: {remarksValue}";
    }
}

// ── Generate Stage PDF ──────────────────────────────────

public record GenerateStagePdfCommand(Guid ProposalId) : IRequest<Result<GeneratePdfResultDto>>;

public record GeneratePdfResultDto(Guid DocumentId, string FilePath);

public class GenerateStagePdfHandler : IRequestHandler<GenerateStagePdfCommand, Result<GeneratePdfResultDto>>
{
    private readonly IPdfGenerationService _pdfService;
    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IRepository<GeneratedDocument> _generatedDocRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public GenerateStagePdfHandler(
        IPdfGenerationService pdfService,
        IRepository<Proposal> proposalRepo,
        IRepository<GeneratedDocument> generatedDocRepo,
        ICurrentUser currentUser,
        IAuditService audit)
    {
        _pdfService = pdfService;
        _proposalRepo = proposalRepo;
        _generatedDocRepo = generatedDocRepo;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<Result<GeneratePdfResultDto>> Handle(GenerateStagePdfCommand request, CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);

        if (proposal is null)
            return Result<GeneratePdfResultDto>.NotFound("Proposal not found");

        // Access: current stage handler, proposer, Commissioner, Auditor, Lotus
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;
        var isProposer = proposal.SubmittedById == userId;
        var isGlobalReader = role is UserRole.Commissioner or UserRole.Auditor or UserRole.Lotus;
        var isStageHandler = IsCurrentStageHandler(proposal.CurrentStage, role);

        if (!isProposer && !isGlobalReader && !isStageHandler)
            return Result<GeneratePdfResultDto>.Forbidden("You do not have access to generate PDF for this proposal");

        var storagePath = await _pdfService.GenerateStageNoteAsync(proposal.Id, cancellationToken);

        var doc = new GeneratedDocument
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            DocumentKind = DocumentKind.StageNote,
            Title_En = $"Stage Note — {proposal.CurrentStage}",
            Title_Alt = $"टप्पा टीप — {proposal.CurrentStage}",
            StoragePath = storagePath,
            GeneratedById = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _generatedDocRepo.AddAsync(doc, cancellationToken);

        await _audit.LogAsync(
            AuditAction.Generate, "GeneratedDocument", doc.Id.ToString(),
            $"Stage PDF generated for proposal {proposal.ProposalNumber} at {proposal.CurrentStage}",
            AuditModule.Document, cancellationToken: cancellationToken);

        return Result<GeneratePdfResultDto>.Success(new GeneratePdfResultDto(doc.Id, storagePath));
    }

    private static bool IsCurrentStageHandler(ProposalStage stage, UserRole role) => stage switch
    {
        ProposalStage.AtCityEngineer => role == UserRole.CityEngineer,
        ProposalStage.AtADO => role == UserRole.ADO,
        ProposalStage.AtChiefAccountant => role == UserRole.ChiefAccountant,
        ProposalStage.AtDeputyCommissioner => role == UserRole.DeputyCommissioner,
        ProposalStage.AtCommissioner => role == UserRole.Commissioner,
        _ => false
    };
}

// ── Save Signature ──────────────────────────────────────

public record SaveSignatureCommand(
    Guid ProposalId,
    long StageHistoryId,
    int PageNumber,
    decimal PositionX,
    decimal PositionY,
    decimal Width,
    decimal Height,
    decimal Rotation,
    string GeneratedPdfPath
) : IRequest<Result<Guid>>;

public class SaveSignatureHandler : IRequestHandler<SaveSignatureCommand, Result<Guid>>
{
    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IRepository<ProposalSignature> _signatureRepo;
    private readonly IRepository<ProposalStageHistory> _historyRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;
    private readonly IPdfSignatureStampService _pdfSignatureStampService;

    public SaveSignatureHandler(
        IRepository<Proposal> proposalRepo,
        IRepository<ProposalSignature> signatureRepo,
        IRepository<ProposalStageHistory> historyRepo,
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService audit,
        IPdfSignatureStampService pdfSignatureStampService)
    {
        _proposalRepo = proposalRepo;
        _signatureRepo = signatureRepo;
        _historyRepo = historyRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _audit = audit;
        _pdfSignatureStampService = pdfSignatureStampService;
    }

    public async Task<Result<Guid>> Handle(SaveSignatureCommand request, CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);
        if (proposal is null)
            return Result<Guid>.NotFound("Proposal not found");

        // Verify the stage history entry belongs to this proposal
        var history = await _historyRepo.Query()
            .FirstOrDefaultAsync(h => h.Id == request.StageHistoryId && h.ProposalId == proposal.Id, cancellationToken);
        if (history is null)
            return Result<Guid>.NotFound("Stage history entry not found");

        // Allow signing if: the user is the one who performed the action, or is the proposer at Draft, or is Lotus
        var role = _currentUser.Role;
        if (role != UserRole.Lotus && history.ActionById != _currentUser.UserId && proposal.SubmittedById != _currentUser.UserId)
            return Result<Guid>.Forbidden("You are not authorized to sign this document");

        var user = await _userRepo.Query().FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (string.IsNullOrWhiteSpace(user?.SignaturePath))
            return Result<Guid>.Failure("Signature image not found in profile");

        var sourceFolder = Path.GetDirectoryName(request.GeneratedPdfPath.Replace('\\', '/'))?.Replace('\\', '/') ?? $"generated/{proposal.Id}";
        var signedPdfPath = await _pdfSignatureStampService.StampSignatureAsync(
            request.GeneratedPdfPath,
            user.SignaturePath,
            request.PageNumber,
            request.PositionX,
            request.PositionY,
            request.Width,
            request.Height,
            request.Rotation,
            sourceFolder,
            $"{proposal.ProposalNumber.Replace("/", "-")}_{history.ToStage}_signed.pdf",
            new SignatureStampContext(
                user?.FullName_En ?? _currentUser.UserName,
                _currentUser.Role.ToString(),
                history.Action is WorkflowAction.Approve
                    ? SignatureTermsText.ApprovalTermsEn
                    : "I agree to the submitted declaration and sign this document in my official capacity.",
                history.Action is WorkflowAction.Approve
                    ? SignatureTermsText.ApprovalTermsAlt
                    : "मी सादर केलेल्या घोषणेस मान्यता देऊन अधिकृत भूमिकेतून स्वाक्षरी करीत आहे.",
                SignatureTermsText.CombineNote(history.Opinion_En, history.Remarks_En),
                SignatureTermsText.CombineNote(history.Opinion_Alt, history.Remarks_Alt)),
            cancellationToken);

        var signature = new ProposalSignature
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            StageHistoryId = request.StageHistoryId,
            SignedById = _currentUser.UserId,
            PageNumber = request.PageNumber,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            Width = request.Width,
            Height = request.Height,
            Rotation = request.Rotation,
            GeneratedPdfPath = signedPdfPath,
            CreatedAt = DateTime.UtcNow
        };

        await _signatureRepo.AddAsync(signature, cancellationToken);

        await _audit.LogAsync(
            AuditAction.Create, "ProposalSignature", signature.Id.ToString(),
            $"Signature placed on proposal {proposal.ProposalNumber} for stage history {request.StageHistoryId}",
            AuditModule.Document, cancellationToken: cancellationToken);

        return Result<Guid>.Success(signature.Id);
    }

    private static bool IsCurrentStageHandler(ProposalStage stage, UserRole role) => stage switch
    {
        ProposalStage.AtCityEngineer => role == UserRole.CityEngineer,
        ProposalStage.AtADO => role == UserRole.ADO,
        ProposalStage.AtChiefAccountant => role == UserRole.ChiefAccountant,
        ProposalStage.AtDeputyCommissioner => role == UserRole.DeputyCommissioner,
        ProposalStage.AtCommissioner => role == UserRole.Commissioner,
        _ => false
    };
}

// ── Get Generated PDFs for a Proposal ───────────────────

public record GetGeneratedPdfsQuery(Guid ProposalId) : IRequest<Result<List<GeneratedPdfDto>>>;

public record GeneratedPdfDto(Guid Id, string DocumentKind, string Title_En, string Title_Alt, string StoragePath, DateTime CreatedAt);

public class GetGeneratedPdfsHandler : IRequestHandler<GetGeneratedPdfsQuery, Result<List<GeneratedPdfDto>>>
{
    private readonly IRepository<Proposal> _proposalRepo;
    private readonly IRepository<GeneratedDocument> _docRepo;
    private readonly ICurrentUser _currentUser;

    public GetGeneratedPdfsHandler(IRepository<Proposal> proposalRepo, IRepository<GeneratedDocument> docRepo, ICurrentUser currentUser)
    {
        _proposalRepo = proposalRepo;
        _docRepo = docRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<GeneratedPdfDto>>> Handle(GetGeneratedPdfsQuery request, CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepo.Query()
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId, cancellationToken);
        if (proposal is null)
            return Result<List<GeneratedPdfDto>>.NotFound("Proposal not found");

        var role = _currentUser.Role;
        var isProposer = proposal.SubmittedById == _currentUser.UserId;
        var isGlobalReader = role is UserRole.Commissioner or UserRole.Auditor or UserRole.Lotus;
        var isStageHandler = IsCurrentStageHandler(proposal.CurrentStage, role);
        if (!isProposer && !isGlobalReader && !isStageHandler)
            return Result<List<GeneratedPdfDto>>.Forbidden();

        var docs = await _docRepo.Query()
            .Where(d => d.ProposalId == request.ProposalId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new GeneratedPdfDto(d.Id, d.DocumentKind.ToString(), d.Title_En, d.Title_Alt, d.StoragePath, d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<GeneratedPdfDto>>.Success(docs);
    }

    private static bool IsCurrentStageHandler(ProposalStage stage, UserRole role) => stage switch
    {
        ProposalStage.AtCityEngineer => role == UserRole.CityEngineer,
        ProposalStage.AtADO => role == UserRole.ADO,
        ProposalStage.AtChiefAccountant => role == UserRole.ChiefAccountant,
        ProposalStage.AtDeputyCommissioner => role == UserRole.DeputyCommissioner,
        ProposalStage.AtCommissioner => role == UserRole.Commissioner,
        _ => false
    };
}
