namespace ProposalManagement.Application.Common.Interfaces;

public record PdfGenerationRequest(
    Guid ProposalId,
    string PdfType // ApprovalOrder, FullProposal
);

public record PdfGenerationResult(
    byte[] Content,
    string FileName,
    string Title_En,
    string? Title_Mr
);

public interface IPdfGenerationService
{
    Task<PdfGenerationResult> GenerateApprovalOrderAsync(Guid proposalId, CancellationToken ct = default);
    Task<PdfGenerationResult> GenerateFullProposalPdfAsync(Guid proposalId, CancellationToken ct = default);
}
