using MediatR;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Dsc;

public record SignDocumentCommand(Guid ProposalId, string DocumentPath) : IRequest<Result<string>>;

public class SignDocumentHandler(
    IAppDbContext db,
    ICurrentUser user,
    IDscService dscService,
    IFileStorageService fileStorage,
    ILogger<SignDocumentHandler> logger)
    : IRequestHandler<SignDocumentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(SignDocumentCommand request, CancellationToken ct)
    {
        var fileContent = await fileStorage.ReadAsync(request.DocumentPath, ct);
        if (fileContent is null) return Result<string>.NotFound("Document not found");

        var hash = System.Security.Cryptography.SHA256.HashData(fileContent);
        var result = await dscService.SignAsync(hash, user.UserId!.Value, ct);

        if (!result.Success)
            return Result<string>.Failure($"DSC signing failed: {result.Error}");

        logger.LogInformation("Document signed via DSC for Proposal {ProposalId} by {UserId}",
            request.ProposalId, user.UserId);
        return Result<string>.Success(result.SignatureReference!);
    }
}

public record VerifySignatureCommand(string DocumentPath, string SignatureReference) : IRequest<Result<bool>>;

public class VerifySignatureHandler(
    IDscService dscService,
    IFileStorageService fileStorage,
    ILogger<VerifySignatureHandler> logger)
    : IRequestHandler<VerifySignatureCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(VerifySignatureCommand request, CancellationToken ct)
    {
        var fileContent = await fileStorage.ReadAsync(request.DocumentPath, ct);
        if (fileContent is null) return Result<bool>.NotFound("Document not found");

        var hash = System.Security.Cryptography.SHA256.HashData(fileContent);
        var isValid = await dscService.VerifyAsync(hash, request.SignatureReference, ct);

        logger.LogInformation("DSC verification result: {IsValid} for {DocumentPath}", isValid, request.DocumentPath);
        return Result<bool>.Success(isValid);
    }
}
