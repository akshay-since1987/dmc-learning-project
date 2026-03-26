namespace ProposalManagement.Application.Common.Interfaces;

public interface IPdfGenerationService
{
    /// <summary>
    /// Generates a stage-note PDF for the given proposal at the current approval stage.
    /// Contains all form data, previous stage notes/remarks, pushback history.
    /// Returns the file path of the generated PDF relative to the storage root.
    /// </summary>
    Task<string> GenerateStageNoteAsync(Guid proposalId, CancellationToken cancellationToken = default);
}
