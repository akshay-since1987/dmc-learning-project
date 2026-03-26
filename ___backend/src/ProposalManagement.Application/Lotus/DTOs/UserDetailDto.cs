namespace ProposalManagement.Application.Lotus.DTOs;

public record UserDetailDto(
    Guid Id, 
    string FullName_En, 
    string FullName_Alt, 
    string MobileNumber, 
    string? Email, 
    string Role, 
    Guid? DepartmentId, 
    string? DepartmentName_En, 
    string? DepartmentName_Alt, 
    Guid? DesignationId, 
    string? DesignationName_En, 
    string? DesignationName_Alt, 
    bool IsActive, 
    string? SignaturePath,
    DateTime CreatedAt, 
    DateTime UpdatedAt);
