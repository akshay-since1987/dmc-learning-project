using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);
        builder.Property(e => e.Code).HasMaxLength(20);
        builder.HasIndex(e => new { e.PalikaId, e.Code }).IsUnique().HasFilter("[Code] IS NOT NULL");

        builder.HasOne(e => e.Palika).WithMany(p => p.Departments).HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DeptWorkCategoryConfiguration : IEntityTypeConfiguration<DeptWorkCategory>
{
    public void Configure(EntityTypeBuilder<DeptWorkCategory> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);

        builder.HasOne(e => e.Department).WithMany(d => d.WorkCategories).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);

        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);
        builder.Property(e => e.Code).HasMaxLength(10);
        builder.Property(e => e.OfficeName_En).HasMaxLength(300);
        builder.Property(e => e.OfficeName_Mr).HasMaxLength(300);
        builder.HasIndex(e => new { e.PalikaId, e.Code }).IsUnique().HasFilter("[Code] IS NOT NULL");

        builder.HasOne(e => e.Palika).WithMany(p => p.Zones).HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrabhagConfiguration : IEntityTypeConfiguration<Prabhag>
{
    public void Configure(EntityTypeBuilder<Prabhag> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);
        builder.HasIndex(e => new { e.PalikaId, e.Number }).IsUnique();

        builder.HasOne(e => e.Palika).WithMany(p => p.Prabhags).HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany(z => z.Prabhags).HasForeignKey(e => e.ZoneId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RequestSourceConfiguration : IEntityTypeConfiguration<RequestSource>
{
    public void Configure(EntityTypeBuilder<RequestSource> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);

        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SiteConditionConfiguration : IEntityTypeConfiguration<SiteCondition>
{
    public void Configure(EntityTypeBuilder<SiteCondition> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(100);
    }
}

public class WorkExecutionMethodConfiguration : IEntityTypeConfiguration<WorkExecutionMethod>
{
    public void Configure(EntityTypeBuilder<WorkExecutionMethod> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);

        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FundTypeConfiguration : IEntityTypeConfiguration<FundType>
{
    public void Configure(EntityTypeBuilder<FundType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(200);

        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BudgetHeadConfiguration : IEntityTypeConfiguration<BudgetHead>
{
    public void Configure(EntityTypeBuilder<BudgetHead> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Code).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(300);
        builder.Property(e => e.FinancialYear).HasMaxLength(20).IsRequired();
        builder.Property(e => e.AllocatedAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.CurrentAvailable).HasColumnType("decimal(18,2)");
        builder.HasIndex(e => new { e.PalikaId, e.DepartmentId, e.FundTypeId, e.Code }).IsUnique();

        builder.HasOne(e => e.Palika).WithMany().HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany(d => d.BudgetHeads).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FundType).WithMany().HasForeignKey(e => e.FundTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
