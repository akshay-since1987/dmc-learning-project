using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;
using ProposalManagement.Domain.Common;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Admin;

// ── Master DTOs ──
public record MasterDetailDto(Guid Id, string Name_En, string? Name_Mr, string? Code, bool IsActive, DateTime CreatedAt);

// ── Generic command for creating/updating a simple master ──
public record SaveMasterCommand : IRequest<Result<Guid>>
{
    public Guid? Id { get; init; }
    public string EntityType { get; init; } = default!;
    public string Name_En { get; init; } = default!;
    public string? Name_Mr { get; init; }
    public string? Code { get; init; }
}

// ── Query: List items for a master entity ──
public record GetMasterItemsQuery(string EntityType, string? Search = null) : IRequest<Result<List<MasterDetailDto>>>;

public class GetMasterItemsHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<GetMasterItemsQuery, Result<List<MasterDetailDto>>>
{
    public async Task<Result<List<MasterDetailDto>>> Handle(GetMasterItemsQuery request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result<List<MasterDetailDto>>.Forbidden();
        var palikaId = user.PalikaId!.Value;

        return request.EntityType switch
        {
            "departments" => Result<List<MasterDetailDto>>.Success(await db.Departments
                .Where(d => d.PalikaId == palikaId)
                .Where(d => string.IsNullOrEmpty(request.Search) || d.Name_En.Contains(request.Search))
                .OrderBy(d => d.Name_En)
                .Select(d => new MasterDetailDto(d.Id, d.Name_En, d.Name_Mr, d.Code, d.IsActive, d.CreatedAt))
                .ToListAsync(ct)),

            "zones" => Result<List<MasterDetailDto>>.Success(await db.Zones
                .Where(z => z.PalikaId == palikaId)
                .Where(z => string.IsNullOrEmpty(request.Search) || z.Name_En.Contains(request.Search))
                .OrderBy(z => z.Name_En)
                .Select(z => new MasterDetailDto(z.Id, z.Name_En, z.Name_Mr, z.Code, z.IsActive, z.CreatedAt))
                .ToListAsync(ct)),

            "designations" => Result<List<MasterDetailDto>>.Success(await db.Designations
                .Where(d => d.PalikaId == palikaId)
                .Where(d => string.IsNullOrEmpty(request.Search) || d.Name_En.Contains(request.Search))
                .OrderBy(d => d.Name_En)
                .Select(d => new MasterDetailDto(d.Id, d.Name_En, d.Name_Mr, null, d.IsActive, d.CreatedAt))
                .ToListAsync(ct)),

            "fund-types" => Result<List<MasterDetailDto>>.Success(await db.FundTypes
                .Where(f => f.PalikaId == palikaId)
                .Where(f => string.IsNullOrEmpty(request.Search) || f.Name_En.Contains(request.Search))
                .OrderBy(f => f.Name_En)
                .Select(f => new MasterDetailDto(f.Id, f.Name_En, f.Name_Mr, null, f.IsActive, f.CreatedAt))
                .ToListAsync(ct)),

            "work-methods" => Result<List<MasterDetailDto>>.Success(await db.WorkExecutionMethods
                .Where(w => w.PalikaId == palikaId)
                .Where(w => string.IsNullOrEmpty(request.Search) || w.Name_En.Contains(request.Search))
                .OrderBy(w => w.Name_En)
                .Select(w => new MasterDetailDto(w.Id, w.Name_En, w.Name_Mr, null, w.IsActive, w.CreatedAt))
                .ToListAsync(ct)),

            "budget-heads" => Result<List<MasterDetailDto>>.Success(await db.BudgetHeads
                .Where(b => b.PalikaId == palikaId)
                .Where(b => string.IsNullOrEmpty(request.Search) || b.Name_En.Contains(request.Search))
                .OrderBy(b => b.Code)
                .Select(b => new MasterDetailDto(b.Id, b.Name_En, b.Name_Mr, b.Code, b.IsActive, b.CreatedAt))
                .ToListAsync(ct)),

            "request-sources" => Result<List<MasterDetailDto>>.Success(await db.RequestSources
                .Where(r => r.PalikaId == palikaId)
                .Where(r => string.IsNullOrEmpty(request.Search) || r.Name_En.Contains(request.Search))
                .OrderBy(r => r.Name_En)
                .Select(r => new MasterDetailDto(r.Id, r.Name_En, r.Name_Mr, null, r.IsActive, r.CreatedAt))
                .ToListAsync(ct)),

            "work-categories" => Result<List<MasterDetailDto>>.Success(await db.DeptWorkCategories
                .Where(c => string.IsNullOrEmpty(request.Search) || c.Name_En.Contains(request.Search))
                .OrderBy(c => c.Name_En)
                .Select(c => new MasterDetailDto(c.Id, c.Name_En, c.Name_Mr, null, c.IsActive, c.CreatedAt))
                .ToListAsync(ct)),

            "site-conditions" => Result<List<MasterDetailDto>>.Success(await db.SiteConditions
                .Where(s => string.IsNullOrEmpty(request.Search) || s.Name_En.Contains(request.Search))
                .OrderBy(s => s.SortOrder)
                .Select(s => new MasterDetailDto(s.Id, s.Name_En, s.Name_Mr, null, true, s.CreatedAt))
                .ToListAsync(ct)),

            _ => Result<List<MasterDetailDto>>.Failure($"Unknown entity type: {request.EntityType}")
        };
    }
}

// ── Command: Save (create/update) a master entity ──
public class SaveMasterHandler(IAppDbContext db, ICurrentUser user, ILogger<SaveMasterHandler> logger)
    : IRequestHandler<SaveMasterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SaveMasterCommand request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result<Guid>.Forbidden();
        if (string.IsNullOrWhiteSpace(request.Name_En)) return Result<Guid>.Failure("Name (English) is required");

        var palikaId = user.PalikaId!.Value;

        return request.EntityType switch
        {
            "departments" => await SaveDepartment(request, palikaId, ct),
            "zones" => await SaveZone(request, palikaId, ct),
            "designations" => await SaveDesignation(request, palikaId, ct),
            "fund-types" => await SaveFundType(request, palikaId, ct),
            "work-methods" => await SaveWorkMethod(request, palikaId, ct),
            "budget-heads" => await SaveBudgetHead(request, palikaId, ct),
            "request-sources" => await SaveRequestSource(request, palikaId, ct),
            "work-categories" => await SaveWorkCategory(request, ct),
            "site-conditions" => await SaveSiteCondition(request, ct),
            _ => Result<Guid>.Failure($"Unknown entity type: {request.EntityType}")
        };
    }

    private async Task<Result<Guid>> SaveDepartment(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.Departments.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.Code = r.Code; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Department {Id} updated", r.Id);
            return Result<Guid>.Success(e.Id);
        }
        var n = new Department { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, Code = r.Code, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Departments.Add(n); await db.SaveChangesAsync(ct);
        logger.LogInformation("Department {Id} created", n.Id);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveZone(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.Zones.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.Code = r.Code; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new Zone { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, Code = r.Code, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Zones.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveDesignation(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.Designations.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new Designation { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Designations.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveFundType(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.FundTypes.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new FundType { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.FundTypes.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveWorkMethod(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.WorkExecutionMethods.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new WorkExecutionMethod { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.WorkExecutionMethods.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveBudgetHead(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.BudgetHeads.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.Code = r.Code; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new BudgetHead { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, Code = r.Code, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.BudgetHeads.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveRequestSource(SaveMasterCommand r, Guid palikaId, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.RequestSources.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new RequestSource { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, PalikaId = palikaId, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.RequestSources.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveWorkCategory(SaveMasterCommand r, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.DeptWorkCategories.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new DeptWorkCategory { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.DeptWorkCategories.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }

    private async Task<Result<Guid>> SaveSiteCondition(SaveMasterCommand r, CancellationToken ct)
    {
        if (r.Id.HasValue) {
            var e = await db.SiteConditions.FindAsync(new object[] { r.Id.Value }, ct);
            if (e is null) return Result<Guid>.NotFound();
            e.Name_En = r.Name_En; e.Name_Mr = r.Name_Mr; e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(e.Id);
        }
        var n = new SiteCondition { Id = Guid.NewGuid(), Name_En = r.Name_En, Name_Mr = r.Name_Mr, SortOrder = 999, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.SiteConditions.Add(n); await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(n.Id);
    }
}

// ── Command: Delete a master entity (soft-delete) ──
public record DeleteMasterCommand(string EntityType, Guid Id) : IRequest<Result>;

public class DeleteMasterHandler(IAppDbContext db, ICurrentUser user)
    : IRequestHandler<DeleteMasterCommand, Result>
{
    public async Task<Result> Handle(DeleteMasterCommand request, CancellationToken ct)
    {
        if (user.Role != "Lotus") return Result.Forbidden();

        return request.EntityType switch
        {
            "departments" => await SoftDelete(db.Departments, request.Id, ct),
            "zones" => await SoftDelete(db.Zones, request.Id, ct),
            "designations" => await SoftDelete(db.Designations, request.Id, ct),
            "fund-types" => await SoftDelete(db.FundTypes, request.Id, ct),
            "work-methods" => await SoftDelete(db.WorkExecutionMethods, request.Id, ct),
            "budget-heads" => await SoftDelete(db.BudgetHeads, request.Id, ct),
            "request-sources" => await SoftDelete(db.RequestSources, request.Id, ct),
            "work-categories" => await SoftDelete(db.DeptWorkCategories, request.Id, ct),
            "site-conditions" => await SoftDelete(db.SiteConditions, request.Id, ct),
            _ => Result.Failure($"Unknown entity type: {request.EntityType}")
        };
    }

    private async Task<Result> SoftDelete<T>(DbSet<T> dbSet, Guid id, CancellationToken ct) where T : BaseAuditableEntity
    {
        var entity = await dbSet.FindAsync(new object[] { id }, ct);
        if (entity is null) return Result.NotFound();
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
