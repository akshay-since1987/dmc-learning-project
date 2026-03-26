using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Proposals.Commands;

public record SubmitProposalCommand(
	Guid Id,
	int SignaturePageNumber,
	decimal SignaturePositionX,
	decimal SignaturePositionY,
	decimal SignatureWidth,
	decimal SignatureHeight,
	decimal SignatureRotation,
	string GeneratedPdfPath
) : IRequest<Result<SubmitProposalResultDto>>;

public record SubmitProposalResultDto(long HistoryId, string NewStage);

public class SubmitProposalCommandValidator : AbstractValidator<SubmitProposalCommand>
{
	public SubmitProposalCommandValidator()
	{
		RuleFor(x => x.Id).NotEmpty();
		RuleFor(x => x.SignaturePageNumber).GreaterThan(0);
		RuleFor(x => x.SignatureWidth).GreaterThan(0);
		RuleFor(x => x.SignatureHeight).GreaterThan(0);
		RuleFor(x => x.GeneratedPdfPath).NotEmpty().MaximumLength(500);
	}
}
