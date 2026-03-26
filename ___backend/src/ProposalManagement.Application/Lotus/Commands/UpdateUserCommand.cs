using FluentValidation;
using MediatR;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Lotus.Commands;

public record UpdateUserCommand(
    Guid Id, string FullName_En, string FullName_Alt, string MobileNumber,
    string? Email, string Role, string? Password,
    Guid? DepartmentId, Guid? DesignationId, bool IsActive) : IRequest<Result>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FullName_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^\d{10}$");
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Role).NotEmpty();
    }
}
