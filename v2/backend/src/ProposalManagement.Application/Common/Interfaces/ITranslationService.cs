namespace ProposalManagement.Application.Common.Interfaces;

public interface ITranslationService
{
    Task<string> TranslateAsync(string text, string sourceLang, string targetLang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> TranslateBatchAsync(IReadOnlyList<string> texts, string sourceLang, string targetLang, CancellationToken cancellationToken = default);
}
