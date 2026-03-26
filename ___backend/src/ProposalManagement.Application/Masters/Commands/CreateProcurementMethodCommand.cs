using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Commands;

public record CreateProcurementMethodCommand(string Name_En, string Name_Alt) : IRequest<Result<Guid>>;

public class CreateProcurementMethodCommandValidator : AbstractValidator<CreateProcurementMethodCommand>
{
    public CreateProcurementMethodCommandValidator()
    {
        RuleFor(x => x.Name_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name_Alt).NotEmpty().MaximumLength(200);
    }
}
