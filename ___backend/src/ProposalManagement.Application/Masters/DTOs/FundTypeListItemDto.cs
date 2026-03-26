namespace ProposalManagement.Application.Masters.DTOs;

public record FundTypeListItemDto(
    Guid Id, string Name_En, string Name_Alt, string? Code, bool IsActive,
    bool IsMnp, bool IsState, bool IsCentral, bool IsDpdc);
