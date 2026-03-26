using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record CreateAccountHeadCommand(string Name_En, string Name_Alt, string Code) : IRequest<Result<Guid>>;

public class CreateAccountHeadCommandValidator : AbstractValidator<CreateAccountHeadCommand>
{
    public CreateAccountHeadCommandValidator()
    {
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
