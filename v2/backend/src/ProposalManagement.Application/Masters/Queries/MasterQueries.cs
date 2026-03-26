using MediatR;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Application.Common.Models;

namespace ProposalManagement.Application.Masters.Queries;

// Generic DTO for any master dropdown
public record MasterItemDto(Guid Id, string Name_En, string? Name_Mr, string? Code = null);

// Departments
public record GetDepartmentsQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetDepartmentsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetDepartmentsQuery request, CancellationToken ct)
    {
        var items = await _db.Departments.Where(d => d.PalikaId == request.PalikaId)
            .Select(d => new MasterItemDto(d.Id, d.Name_En, d.Name_Mr, d.Code)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// Zones
public record GetZonesQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetZonesHandler : IRequestHandler<GetZonesQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetZonesHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetZonesQuery request, CancellationToken ct)
    {
        var items = await _db.Zones.Where(z => z.PalikaId == request.PalikaId)
            .Select(z => new MasterItemDto(z.Id, z.Name_En, z.Name_Mr, z.Code)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// Prabhags
public record GetPrabhagsQuery(Guid PalikaId, Guid? ZoneId = null) : IRequest<Result<List<MasterItemDto>>>;
public class GetPrabhagsHandler : IRequestHandler<GetPrabhagsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetPrabhagsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetPrabhagsQuery request, CancellationToken ct)
    {
        var query = _db.Prabhags.Where(p => p.PalikaId == request.PalikaId);
        if (request.ZoneId.HasValue) query = query.Where(p => p.ZoneId == request.ZoneId.Value);
        var items = await query.OrderBy(p => p.Number)
            .Select(p => new MasterItemDto(p.Id, p.Name_En, p.Name_Mr, p.Number.ToString())).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// Designations
public record GetDesignationsQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetDesignationsHandler : IRequestHandler<GetDesignationsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetDesignationsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetDesignationsQuery request, CancellationToken ct)
    {
        var items = await _db.Designations.Where(d => d.PalikaId == request.PalikaId)
            .Select(d => new MasterItemDto(d.Id, d.Name_En, d.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// FundTypes
public record GetFundTypesQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetFundTypesHandler : IRequestHandler<GetFundTypesQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetFundTypesHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetFundTypesQuery request, CancellationToken ct)
    {
        var items = await _db.FundTypes.Where(f => f.PalikaId == request.PalikaId)
            .Select(f => new MasterItemDto(f.Id, f.Name_En, f.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// WorkExecutionMethods
public record GetWorkMethodsQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetWorkMethodsHandler : IRequestHandler<GetWorkMethodsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetWorkMethodsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetWorkMethodsQuery request, CancellationToken ct)
    {
        var items = await _db.WorkExecutionMethods.Where(w => w.PalikaId == request.PalikaId)
            .Select(w => new MasterItemDto(w.Id, w.Name_En, w.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// SiteConditions (global, no PalikaId)
public record GetSiteConditionsQuery : IRequest<Result<List<MasterItemDto>>>;
public class GetSiteConditionsHandler : IRequestHandler<GetSiteConditionsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetSiteConditionsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetSiteConditionsQuery request, CancellationToken ct)
    {
        var items = await _db.SiteConditions.OrderBy(s => s.SortOrder)
            .Select(s => new MasterItemDto(s.Id, s.Name_En, s.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// RequestSources
public record GetRequestSourcesQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetRequestSourcesHandler : IRequestHandler<GetRequestSourcesQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetRequestSourcesHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetRequestSourcesQuery request, CancellationToken ct)
    {
        var items = await _db.RequestSources.Where(r => r.PalikaId == request.PalikaId)
            .Select(r => new MasterItemDto(r.Id, r.Name_En, r.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// DeptWorkCategories
public record GetWorkCategoriesQuery(Guid? DepartmentId) : IRequest<Result<List<MasterItemDto>>>;
public class GetWorkCategoriesHandler : IRequestHandler<GetWorkCategoriesQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetWorkCategoriesHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetWorkCategoriesQuery request, CancellationToken ct)
    {
        var query = _db.DeptWorkCategories.AsQueryable();
        if (request.DepartmentId.HasValue)
            query = query.Where(c => c.DepartmentId == request.DepartmentId.Value || c.DepartmentId == null);
        else
            query = query.Where(c => c.DepartmentId == null);
        var items = await query.Select(c => new MasterItemDto(c.Id, c.Name_En, c.Name_Mr)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// BudgetHeads
public record GetBudgetHeadsQuery(Guid PalikaId) : IRequest<Result<List<MasterItemDto>>>;
public class GetBudgetHeadsHandler : IRequestHandler<GetBudgetHeadsQuery, Result<List<MasterItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetBudgetHeadsHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<MasterItemDto>>> Handle(GetBudgetHeadsQuery request, CancellationToken ct)
    {
        var items = await _db.BudgetHeads.Where(b => b.PalikaId == request.PalikaId)
            .Select(b => new MasterItemDto(b.Id, b.Name_En, b.Name_Mr, b.Code)).ToListAsync(ct);
        return Result<List<MasterItemDto>>.Success(items);
    }
}

// Users by role
public record UserItemDto(Guid Id, string FullName_En, string? FullName_Mr, string Role, string? DepartmentName);
public record GetUsersByRoleQuery(string Role, Guid PalikaId) : IRequest<Result<List<UserItemDto>>>;
public class GetUsersByRoleHandler : IRequestHandler<GetUsersByRoleQuery, Result<List<UserItemDto>>>
{
    private readonly IAppDbContext _db;
    public GetUsersByRoleHandler(IAppDbContext db) => _db = db;
    public async Task<Result<List<UserItemDto>>> Handle(GetUsersByRoleQuery request, CancellationToken ct)
    {
        var items = await _db.Users
            .Where(u => u.Role == request.Role && u.PalikaId == request.PalikaId)
            .Select(u => new UserItemDto(u.Id, u.FullName_En, u.FullName_Mr, u.Role, u.Department != null ? u.Department.Name_En : null))
            .ToListAsync(ct);
        return Result<List<UserItemDto>>.Success(items);
    }
}
