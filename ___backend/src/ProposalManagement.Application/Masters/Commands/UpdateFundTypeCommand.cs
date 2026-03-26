using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record UpdateFundTypeCommand(Guid Id, string Name_En, string Name_Alt, string Code, bool IsActive, bool IsMnp = false, bool IsState = false, bool IsCentral = false, bool IsDpdc = false) : IRequest<Result>;

public class UpdateFundTypeCommandValidator : AbstractValidator<UpdateFundTypeCommand>
{
    public UpdateFundTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
