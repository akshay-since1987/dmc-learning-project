using Microsoft.AspNetCore.Authorization;

namespace ProposalManagement.Api.Filters;

public class LotusAuthorizeAttribute : AuthorizeAttribute
{
    public LotusAuthorizeAttribute() : base()
    {
        Roles = "Lotus";
    }
}
