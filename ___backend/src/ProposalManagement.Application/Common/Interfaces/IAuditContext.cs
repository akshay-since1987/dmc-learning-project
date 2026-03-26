namespace ProposalManagement.Application.Common.Interfaces;

public interface IAuditContext
{
    string IpAddress { get; set; }
    string UserAgent { get; set; }
    Guid? UserId { get; set; }
    string UserName { get; set; }
    string UserRole { get; set; }
}
