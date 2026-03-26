using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Lotus.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IRepository<User> _repo;

    public UpdateUserCommandHandler(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure("User not found", 404);

        // Check duplicate mobile (excluding self)
        var duplicate = await _repo.QueryIgnoreFilters()
            .AnyAsync(u => u.MobileNumber == request.MobileNumber && u.Id != request.Id, cancellationToken);
        if (duplicate)
            return Result.Failure($"Mobile number '{request.MobileNumber}' already in use", 409);

        if (!Enum.TryParse<UserRole>(request.Role, out var role))
            return Result.Failure("Invalid role", 400);

        user.FullName_En = request.FullName_En;
        user.FullName_Alt = request.FullName_Alt;
        user.MobileNumber = request.MobileNumber;
        user.Email = request.Email;
        user.Role = role;
        user.DepartmentId = request.DepartmentId;
        user.DesignationId = request.DesignationId;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        // Update password if provided
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _repo.UpdateAsync(user, cancellationToken);
        return Result.Success();
    }
}
