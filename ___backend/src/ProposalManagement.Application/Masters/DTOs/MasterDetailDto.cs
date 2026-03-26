namespace ProposalManagement.Application.Masters.DTOs;

public record MasterDetailDto(Guid Id, string Name_En, string Name_Alt, string? Code, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);
