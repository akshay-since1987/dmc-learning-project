using FluentValidation;
using ProposalManagement.Application.Proposals.Commands;
using ProposalManagement.Application.Workflow.Commands;

namespace ProposalManagement.Application.Proposals.Validators;

public class CreateProposalValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DeptWorkCategoryId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.PrabhagId).NotEmpty();
        RuleFor(x => x.WorkTitle_En).NotEmpty().MaximumLength(500);
        RuleFor(x => x.WorkDescription_En).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Priority).NotEmpty().Must(p => p is "High" or "Medium" or "Low");
    }
}

public class UpdateProposalValidator : AbstractValidator<UpdateProposalCommand>
{
    public UpdateProposalValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DeptWorkCategoryId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.PrabhagId).NotEmpty();
        RuleFor(x => x.WorkTitle_En).NotEmpty().MaximumLength(500);
        RuleFor(x => x.WorkDescription_En).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Priority).NotEmpty().Must(p => p is "High" or "Medium" or "Low");
    }
}

public class PushBackValidator : AbstractValidator<PushBackProposalCommand>
{
    public PushBackValidator()
    {
        RuleFor(x => x.ProposalId).NotEmpty();
        RuleFor(x => x.PushBackNote_En).NotEmpty().WithMessage("Push back note is mandatory");
    }
}

public class ApproveValidator : AbstractValidator<ApproveProposalCommand>
{
    public ApproveValidator()
    {
        RuleFor(x => x.ProposalId).NotEmpty();
        RuleFor(x => x.DisclaimerAccepted).Equal(true).WithMessage("Disclaimer must be accepted");
    }
}
