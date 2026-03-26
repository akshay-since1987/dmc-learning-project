using FluentValidation;

namespace ProposalManagement.Application.Workflow.Commands;

public class PushBackStageCommandValidator : AbstractValidator<PushBackStageCommand>
{
    public PushBackStageCommandValidator()
    {
        RuleFor(x => x.ProposalId).NotEmpty();
        RuleFor(x => x.TargetStage).NotEmpty().WithMessage("Target stage is required");
        RuleFor(x => x.Reason_En).NotEmpty().WithMessage("Push-back reason (English) is mandatory");
        RuleFor(x => x.Reason_En).MaximumLength(4000);
        RuleFor(x => x.Reason_Alt).MaximumLength(4000).When(x => x.Reason_Alt != null);
        RuleFor(x => x.Opinion_En).MaximumLength(4000).When(x => x.Opinion_En != null);
        RuleFor(x => x.Remarks_En).MaximumLength(4000).When(x => x.Remarks_En != null);
    }
}
