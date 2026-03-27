namespace ProposalManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>Save file and return the relative URL path (e.g. /uploads/proposals/{guid}/file.pdf)</summary>
    Task<string> SaveAsync(string folder, string fileName, byte[] content, CancellationToken ct = default);

    /// <summary>Delete a file by its relative URL path</summary>
    Task DeleteAsync(string relativePath, CancellationToken ct = default);

    /// <summary>Read file bytes by relative URL path</summary>
    Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default);

    /// <summary>Check if file exists</summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);
}
