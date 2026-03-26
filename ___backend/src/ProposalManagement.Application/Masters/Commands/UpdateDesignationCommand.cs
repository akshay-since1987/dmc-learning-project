using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record UpdateDesignationCommand(Guid Id, string Name_En, string Name_Alt, bool IsActive) : IRequest<Result>;

public class UpdateDesignationCommandValidator : AbstractValidator<UpdateDesignationCommand>
{
    public UpdateDesignationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
    }
}
