namespace ProposalManagement.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string role, Guid palikaId);
    string GenerateRefreshToken();
}
