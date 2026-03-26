using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record UpdateWardCommand(Guid Id, int Number, string Name_En, string Name_Alt, bool IsActive) : IRequest<Result>;

public class UpdateWardCommandValidator : AbstractValidator<UpdateWardCommand>
{
    public UpdateWardCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Number).GreaterThan(0);
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
    }
}
