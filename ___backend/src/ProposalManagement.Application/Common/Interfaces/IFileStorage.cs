namespace ProposalManagement.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
}
