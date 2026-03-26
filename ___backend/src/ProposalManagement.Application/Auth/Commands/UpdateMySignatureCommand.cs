using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Auth.Commands;

public record UpdateMySignatureCommand(string SignaturePath) : IRequest<Result<UserDto>>;

public class UpdateMySignatureCommandHandler : IRequestHandler<UpdateMySignatureCommand, Result<UserDto>>
{
    private readonly IRepository<User> _userRepo;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public UpdateMySignatureCommandHandler(
        IRepository<User> userRepo,
        ICurrentUser currentUser,
        IAuditService auditService)
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    public async Task<Result<UserDto>> Handle(UpdateMySignatureCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            return Result<UserDto>.Unauthorized();

        var user = await _userRepo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        if (user is null || user.IsDeleted || !user.IsActive)
            return Result<UserDto>.NotFound("User not found");

        var oldPath = user.SignaturePath;
        user.SignaturePath = request.SignaturePath;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user, cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Upload,
            "UserSignature",
            user.Id.ToString(),
            "User uploaded/updated own signature",
            AuditModule.Auth,
            AuditSeverity.Info,
            oldPath,
            user.SignaturePath,
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
