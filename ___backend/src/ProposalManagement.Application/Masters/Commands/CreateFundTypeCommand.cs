using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record CreateFundTypeCommand(string Name_En, string Name_Alt, string Code, bool IsMnp = false, bool IsState = false, bool IsCentral = false, bool IsDpdc = false) : IRequest<Result<Guid>>;

public class CreateFundTypeCommandValidator : AbstractValidator<CreateFundTypeCommand>
{
    public CreateFundTypeCommandValidator()
    {
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
