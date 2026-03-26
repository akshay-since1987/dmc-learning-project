using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ProposalManagement.Application.Common.Interfaces;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth & Users
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Masters
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<FundType> FundTypes => Set<FundType>();
    public DbSet<AccountHead> AccountHeads => Set<AccountHead>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<ProcurementMethod> ProcurementMethods => Set<ProcurementMethod>();
    public DbSet<TenderPublicationPeriod> TenderPublicationPeriods => Set<TenderPublicationPeriod>();

    // Corporation
    public DbSet<CorporationSettings> CorporationSettings => Set<CorporationSettings>();

    // Proposals
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalDocument> ProposalDocuments => Set<ProposalDocument>();
    public DbSet<ProposalStageHistory> ProposalStageHistory => Set<ProposalStageHistory>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();
    public DbSet<ProposalSignature> ProposalSignatures => Set<ProposalSignature>();
    public DbSet<ProposalStepLock> ProposalStepLocks => Set<ProposalStepLock>();

    // Audit & Notifications
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
