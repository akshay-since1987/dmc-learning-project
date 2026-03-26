using System.Security.Claims;
using ProposalManagement.Application.Common.Interfaces;

namespace ProposalManagement.Api.Middleware;

public class AuditContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditContext auditContext)
    {
        auditContext.IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        auditContext.UserAgent = context.Request.Headers.UserAgent.ToString();

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirst("sub");
            if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                auditContext.UserId = userId;
            }

            auditContext.UserName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            auditContext.UserRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        await _next(context);
    }
}
