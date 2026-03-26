namespace ProposalManagement.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Role { get; }
    Guid? PalikaId { get; }
    bool IsAuthenticated { get; }
}
