namespace ProposalManagement.Application.Lotus.DTOs;

public record UserListDto(
    Guid Id, 
    string FullName_En, 
    string FullName_Alt, 
    string MobileNumber, 
    string? Email, 
    string Role, 
    Guid? DepartmentId, 
    string? DepartmentName_En, 
    Guid? DesignationId, 
    string? DesignationName_En, 
    bool IsActive,
    string? SignaturePath);
