using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record UpdateAccountHeadCommand(Guid Id, string Name_En, string Name_Alt, string Code, bool IsActive) : IRequest<Result>;

public class UpdateAccountHeadCommandValidator : AbstractValidator<UpdateAccountHeadCommand>
{
    public UpdateAccountHeadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
