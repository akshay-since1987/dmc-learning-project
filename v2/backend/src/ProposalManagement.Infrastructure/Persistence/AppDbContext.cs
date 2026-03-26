using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Palika> Palikas => Set<Palika>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DeptWorkCategory> DeptWorkCategories => Set<DeptWorkCategory>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Prabhag> Prabhags => Set<Prabhag>();
    public DbSet<RequestSource> RequestSources => Set<RequestSource>();
    public DbSet<SiteCondition> SiteConditions => Set<SiteCondition>();
    public DbSet<WorkExecutionMethod> WorkExecutionMethods => Set<WorkExecutionMethod>();
    public DbSet<FundType> FundTypes => Set<FundType>();
    public DbSet<BudgetHead> BudgetHeads => Set<BudgetHead>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalDocument> ProposalDocuments => Set<ProposalDocument>();
    public DbSet<FieldVisit> FieldVisits => Set<FieldVisit>();
    public DbSet<FieldVisitPhoto> FieldVisitPhotos => Set<FieldVisitPhoto>();
    public DbSet<Estimate> Estimates => Set<Estimate>();
    public DbSet<TechnicalSanction> TechnicalSanctions => Set<TechnicalSanction>();
    public DbSet<PramaDetail> PramaDetails => Set<PramaDetail>();
    public DbSet<BudgetDetail> BudgetDetails => Set<BudgetDetail>();
    public DbSet<ProposalApproval> ProposalApprovals => Set<ProposalApproval>();
    public DbSet<GeneratedPdf> GeneratedPdfs => Set<GeneratedPdf>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
