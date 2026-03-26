using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string UserName { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
