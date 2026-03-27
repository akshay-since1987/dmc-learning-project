namespace ProposalManagement.Application.Common.Interfaces;

public record SignatureStampContext(
    string SignerName,
    string SignerRole,
    string? Terms_En,
    string? Terms_Alt,
    string? Note_En,
    string? Note_Alt);

public interface IPdfSignatureStampService
{
    Task<string> StampSignatureAsync(
        string sourcePdfPath,
        string signatureImagePath,
        int pageNumber,
        decimal positionX,
        decimal positionY,
        decimal width,
        decimal height,
        decimal rotation,
        string outputFolder,
        string outputFileName,
        SignatureStampContext? context = null,
        CancellationToken cancellationToken = default);
}
