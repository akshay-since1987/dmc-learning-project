using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UserId
    {
        get
        {
            var id = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return id is not null ? Guid.Parse(id) : null;
        }
    }

    public string? Role => _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public Guid? PalikaId
    {
        get
        {
            var id = _accessor.HttpContext?.User?.FindFirstValue("PalikaId");
            return id is not null ? Guid.Parse(id) : null;
        }
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
