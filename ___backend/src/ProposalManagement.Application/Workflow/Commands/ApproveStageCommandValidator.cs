using FluentValidation;

namespace ProposalManagement.Application.Workflow.Commands;

public class ApproveStageCommandValidator : AbstractValidator<ApproveStageCommand>
{
    public ApproveStageCommandValidator()
    {
        RuleFor(x => x.ProposalId).NotEmpty();
        RuleFor(x => x.TermsAccepted).Equal(true).WithMessage("You must agree to the terms before approval.");
        RuleFor(x => x.Opinion_En).MaximumLength(4000).When(x => x.Opinion_En != null);
        RuleFor(x => x.Remarks_En).MaximumLength(4000).When(x => x.Remarks_En != null);
    }
}
