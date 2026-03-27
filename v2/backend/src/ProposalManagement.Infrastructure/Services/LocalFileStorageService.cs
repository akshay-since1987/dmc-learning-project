using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

/// <summary>Stores files on local disk under wwwroot/uploads/</summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath = "wwwroot")
    {
        _basePath = basePath;
    }

    public async Task<string> SaveAsync(string folder, string fileName, byte[] content, CancellationToken ct = default)
    {
        var safeFileName = Path.GetFileName(fileName);
        var storageName = $"{Guid.NewGuid():N}_{safeFileName}";
        var diskFolder = Path.Combine(_basePath, "uploads", folder);
        Directory.CreateDirectory(diskFolder);
        var diskPath = Path.Combine(diskFolder, storageName);

        await File.WriteAllBytesAsync(diskPath, content, ct);

        return $"/uploads/{folder}/{storageName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.CompletedTask;

        var diskPath = Path.Combine(_basePath, relativePath.TrimStart('/'));
        if (File.Exists(diskPath)) File.Delete(diskPath);

        return Task.CompletedTask;
    }

    public async Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        var diskPath = Path.Combine(_basePath, relativePath.TrimStart('/'));
        if (!File.Exists(diskPath)) return null;

        return await File.ReadAllBytesAsync(diskPath, ct);
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return Task.FromResult(false);

        var diskPath = Path.Combine(_basePath, relativePath.TrimStart('/'));
        return Task.FromResult(File.Exists(diskPath));
    }
}
