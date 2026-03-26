using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Auth.Commands;

public record UpdateMyProfileCommand(string FullName_En, string FullName_Alt, string? Email) : IRequest<Result<UserDto>>;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FullName_En).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FullName_Alt).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, Result<UserDto>>
{
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public UpdateMyProfileCommandHandler(
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<UserDto>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            return Result<UserDto>.Unauthorized();

        var user = await _userRepo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user is null || user.IsDeleted || !user.IsActive)
            return Result<UserDto>.NotFound("User not found");

        var oldValues = $"{{\"fullName_En\":\"{user.FullName_En}\",\"fullName_Alt\":\"{user.FullName_Alt}\",\"email\":\"{user.Email}\"}}";

        user.FullName_En = request.FullName_En.Trim();
        user.FullName_Alt = request.FullName_Alt.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user, cancellationToken);

        var newValues = $"{{\"fullName_En\":\"{user.FullName_En}\",\"fullName_Alt\":\"{user.FullName_Alt}\",\"email\":\"{user.Email}\"}}";
        await _auditService.LogAsync(
            AuditAction.Update,
            "User",
            user.Id.ToString(),
            "User updated own profile",
            AuditModule.Auth,
            AuditSeverity.Info,
            oldValues,
            newValues,
            cancellationToken: cancellationToken);

        var dto = new UserDto(
            user.Id,
            user.FullName_En,
            user.FullName_Alt,
            user.MobileNumber,
            user.Email,
            user.Role.ToString(),
            user.DepartmentId,
            user.DesignationId,
            user.SignaturePath);

        return Result<UserDto>.Success(dto);
    }
}
