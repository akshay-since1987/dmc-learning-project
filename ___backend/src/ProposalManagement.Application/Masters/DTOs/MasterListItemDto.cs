namespace ProposalManagement.Application.Masters.DTOs;

public record MasterListItemDto(Guid Id, string Name_En, string Name_Alt, string? Code, bool IsActive);
