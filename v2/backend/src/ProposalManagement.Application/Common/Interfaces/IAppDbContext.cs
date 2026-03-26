using Microsoft.EntityFrameworkCore;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Palika> Palikas { get; }
    DbSet<User> Users { get; }
    DbSet<OtpRequest> OtpRequests { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Department> Departments { get; }
    DbSet<DeptWorkCategory> DeptWorkCategories { get; }
    DbSet<Designation> Designations { get; }
    DbSet<Zone> Zones { get; }
    DbSet<Prabhag> Prabhags { get; }
    DbSet<RequestSource> RequestSources { get; }
    DbSet<SiteCondition> SiteConditions { get; }
    DbSet<WorkExecutionMethod> WorkExecutionMethods { get; }
    DbSet<FundType> FundTypes { get; }
    DbSet<BudgetHead> BudgetHeads { get; }
    DbSet<Proposal> Proposals { get; }
    DbSet<ProposalDocument> ProposalDocuments { get; }
    DbSet<FieldVisit> FieldVisits { get; }
    DbSet<FieldVisitPhoto> FieldVisitPhotos { get; }
    DbSet<Estimate> Estimates { get; }
    DbSet<TechnicalSanction> TechnicalSanctions { get; }
    DbSet<PramaDetail> PramaDetails { get; }
    DbSet<BudgetDetail> BudgetDetails { get; }
    DbSet<ProposalApproval> ProposalApprovals { get; }
    DbSet<GeneratedPdf> GeneratedPdfs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditTrail> AuditTrails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
