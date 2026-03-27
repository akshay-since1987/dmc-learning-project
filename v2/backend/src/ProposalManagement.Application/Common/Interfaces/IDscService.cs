namespace ProposalManagement.Application.Common.Interfaces;

public record DscSignResult(bool Success, string? SignatureReference, string? Error);

public interface IDscService
{
    /// <summary>Sign a document hash using DSC. Returns signature reference or error.</summary>
    Task<DscSignResult> SignAsync(byte[] documentHash, Guid userId, CancellationToken ct = default);

    /// <summary>Verify a DSC signature reference against a document hash.</summary>
    Task<bool> VerifyAsync(byte[] documentHash, string signatureReference, CancellationToken ct = default);
}
