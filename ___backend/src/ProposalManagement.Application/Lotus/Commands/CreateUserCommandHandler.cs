using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Lotus.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IRepository<User> _repo;

    public CreateUserCommandHandler(IRepository<User> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repo.QueryIgnoreFilters()
            .AnyAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"User with mobile number '{request.MobileNumber}' already exists", 409);

        if (!Enum.TryParse<UserRole>(request.Role, out var role))
            return Result<Guid>.Failure("Invalid role", 400);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName_En = request.FullName_En,
            FullName_Alt = request.FullName_Alt,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            Role = role,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            PasswordHash = role == UserRole.Lotus && !string.IsNullOrEmpty(request.Password)
                ? BCrypt.Net.BCrypt.HashPassword(request.Password)
                : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(user, cancellationToken);
        return Result<Guid>.Success(user.Id);
    }
}
