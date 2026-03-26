using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Application.Notifications;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Admin;

// ── User DTOs ──
public record AdminUserDto(Guid Id, string FullName_En, string? FullName_Mr, string MobileNumber,
    string? Email, string Role, Guid? DepartmentId, string? DepartmentName,
    Guid? DesignationId, string? DesignationName, bool IsActive, DateTime CreatedAt);

public record AdminUserListDto(Guid Id, string FullName_En, string MobileNumber, string Role,
    string? DepartmentName, bool IsActive);

// ── Query: List users ──
public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Role = null, string? Search = null) 
    : IRequest<Result<PagedList<AdminUserListDto>>>;

public class GetUsersHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<GetUsersQuery, Result<PagedList<AdminUserListDto>>>
{
    public async Task<Result<PagedList<AdminUserListDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result<PagedList<AdminUserListDto>>.Forbidden();

        var q = db.Users.Where(u => !u.IsDeleted).Include(u => u.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Role)) q = q.Where(u => u.Role == request.Role);
        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(u => u.FullName_En.Contains(request.Search) || u.MobileNumber.Contains(request.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(u => u.FullName_En)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(u => new AdminUserListDto(u.Id, u.FullName_En, u.MobileNumber, u.Role,
                u.Department != null ? u.Department.Name_En : null, u.IsActive))
            .ToListAsync(ct);

        return Result<PagedList<AdminUserListDto>>.Success(new(items, total, request.Page, request.PageSize));
    }
}

// ── Query: Get user by ID ──
public record GetUserByIdQuery(Guid Id) : IRequest<Result<AdminUserDto>>;

public class GetUserByIdHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<GetUserByIdQuery, Result<AdminUserDto>>
{
    public async Task<Result<AdminUserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result<AdminUserDto>.Forbidden();

        var u = await db.Users.Include(x => x.Department).Include(x => x.Designation)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, ct);
        if (u is null) return Result<AdminUserDto>.NotFound();

        return Result<AdminUserDto>.Success(new AdminUserDto(u.Id, u.FullName_En, u.FullName_Mr, u.MobileNumber,
            u.Email, u.Role, u.DepartmentId, u.Department?.Name_En,
            u.DesignationId, u.Designation?.Name_En, u.IsActive, u.CreatedAt));
    }
}

// ── Command: Create/Update user ──
public record SaveUserCommand : IRequest<Result<Guid>>
{
    public Guid? Id { get; init; }
    public string FullName_En { get; init; } = default!;
    public string? FullName_Mr { get; init; }
    public string MobileNumber { get; init; } = default!;
    public string? Email { get; init; }
    public string Role { get; init; } = default!;
    public Guid? DepartmentId { get; init; }
    public Guid? DesignationId { get; init; }
    public bool IsActive { get; init; } = true;
}

public class SaveUserHandler(IAppDbContext db, ICurrentUser currentUser, ILogger<SaveUserHandler> logger) 
    : IRequestHandler<SaveUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveUserCommand request, CancellationToken ct)
    {
        if (currentUser.Role != "Lotus") return Result<Guid>.Forbidden();

        if (request.Id.HasValue)
        {
            var existing = await db.Users.FindAsync(new object[] { request.Id.Value }, ct);
            if (existing is null) return Result<Guid>.NotFound();

            // Check mobile uniqueness
            var dup = await db.Users.AnyAsync(u => u.MobileNumber == request.MobileNumber && u.Id != request.Id && !u.IsDeleted, ct);
            if (dup) return Result<Guid>.Failure("Mobile number already in use");

            existing.FullName_En = request.FullName_En;
            existing.FullName_Mr = request.FullName_Mr;
            existing.MobileNumber = request.MobileNumber;
            existing.Email = request.Email;
            existing.Role = request.Role;
            existing.DepartmentId = request.DepartmentId;
            existing.DesignationId = request.DesignationId;
            existing.IsActive = request.IsActive;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("User {UserId} updated by Lotus", request.Id);
            return Result<Guid>.Success(existing.Id);
        }

        // Check mobile uniqueness for new user
        var exists = await db.Users.AnyAsync(u => u.MobileNumber == request.MobileNumber && !u.IsDeleted, ct);
        if (exists) return Result<Guid>.Failure("Mobile number already in use");

        var newUser = new User
        {
            Id = Guid.NewGuid(), FullName_En = request.FullName_En, FullName_Mr = request.FullName_Mr,
            MobileNumber = request.MobileNumber, Email = request.Email, Role = request.Role,
            DepartmentId = request.DepartmentId, DesignationId = request.DesignationId,
            PalikaId = currentUser.PalikaId!.Value, IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(newUser);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("User {UserId} created by Lotus", newUser.Id);
        return Result<Guid>.Success(newUser.Id);
    }
}

// ── Command: Toggle user active ──
public record ToggleUserActiveCommand(Guid Id) : IRequest<Result>;

public class ToggleUserActiveHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<ToggleUserActiveCommand, Result>
{
    public async Task<Result> Handle(ToggleUserActiveCommand request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result.Forbidden();
        var u = await db.Users.FindAsync(new object[] { request.Id }, ct);
        if (u is null) return Result.NotFound();
        u.IsActive = !u.IsActive;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Command: Delete user (soft) ──
public record DeleteUserCommand(Guid Id) : IRequest<Result>;

public class DeleteUserHandler(IAppDbContext db, ICurrentUser user) : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result.Forbidden();
        var u = await db.Users.FindAsync(new object[] { request.Id }, ct);
        if (u is null) return Result.NotFound();
        u.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
