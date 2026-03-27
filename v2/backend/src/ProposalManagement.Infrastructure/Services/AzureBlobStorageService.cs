using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

/// <summary>Stores files in Azure Blob Storage (production)</summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(string connectionString, string containerName = "uploads")
    {
        var blobService = new BlobServiceClient(connectionString);
        _container = blobService.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> SaveAsync(string folder, string fileName, byte[] content, CancellationToken ct = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var blobName = $"{folder}/{Guid.NewGuid():N}_{safeFileName}";
        var blob = _container.GetBlobClient(blobName);

        using var stream = new MemoryStream(content);
        await blob.UploadAsync(stream, overwrite: true, ct);

        return $"/uploads/{blobName}";
    }

    public async Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var blobName = relativePath.TrimStart('/').Replace("uploads/", "", StringComparison.OrdinalIgnoreCase);
        var blob = _container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        var blobName = relativePath.TrimStart('/').Replace("uploads/", "", StringComparison.OrdinalIgnoreCase);
        var blob = _container.GetBlobClient(blobName);

        if (!await blob.ExistsAsync(ct)) return null;

        var download = await blob.DownloadContentAsync(ct);
        return download.Value.Content.ToArray();
    }

    public async Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return false;

        var blobName = relativePath.TrimStart('/').Replace("uploads/", "", StringComparison.OrdinalIgnoreCase);
        var blob = _container.GetBlobClient(blobName);
        var response = await blob.ExistsAsync(ct);
        return response.Value;
    }
}
