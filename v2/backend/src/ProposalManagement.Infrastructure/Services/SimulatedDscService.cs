using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

/// <summary>Simulated DSC service for development. Generates deterministic mock signatures.</summary>
public class SimulatedDscService(ILogger<SimulatedDscService> logger) : IDscService
{
    public Task<DscSignResult> SignAsync(byte[] documentHash, Guid userId, CancellationToken ct = default)
    {
        // Generate a deterministic mock signature reference from hash + userId
        var combined = new byte[documentHash.Length + 16];
        documentHash.CopyTo(combined, 0);
        userId.ToByteArray().CopyTo(combined, documentHash.Length);

        var sigBytes = SHA256.HashData(combined);
        var sigRef = $"SIM-DSC-{Convert.ToHexString(sigBytes[..16])}";

        logger.LogInformation("Simulated DSC sign for UserId {UserId}, ref: {SignatureRef}", userId, sigRef);
        return Task.FromResult(new DscSignResult(true, sigRef, null));
    }

    public Task<bool> VerifyAsync(byte[] documentHash, string signatureReference, CancellationToken ct = default)
    {
        // In simulation mode, accept any signature that starts with our prefix
        var valid = signatureReference?.StartsWith("SIM-DSC-", StringComparison.OrdinalIgnoreCase) == true;
        logger.LogInformation("Simulated DSC verify: {Valid} for ref {SignatureRef}", valid, signatureReference);
        return Task.FromResult(valid);
    }
}
