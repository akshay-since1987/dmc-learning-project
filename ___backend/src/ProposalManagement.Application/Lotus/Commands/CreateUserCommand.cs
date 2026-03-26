using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Lotus.Commands;

public record CreateUserCommand(
    string FullName_En, string FullName_Alt, string MobileNumber, 
    string? Email, string Role, string? Password, 
    Guid? DepartmentId, Guid? DesignationId) : IRequest<Result<Guid>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FullName_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FullName_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^\d{10}$").WithMessage("Mobile number must be 10 digits");
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Role).NotEmpty().Must(r => new[] { "Submitter", "CityEngineer", "ChiefAccountant", "DeputyCommissioner", "Commissioner", "Auditor", "Lotus" }.Contains(r)).WithMessage("Invalid role");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).When(x => x.Role == "Lotus").WithMessage("Password is required for Lotus users");
    }
}
