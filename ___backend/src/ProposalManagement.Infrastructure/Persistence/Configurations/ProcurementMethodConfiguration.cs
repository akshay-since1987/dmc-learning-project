using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class ProcurementMethodConfiguration : IEntityTypeConfiguration<ProcurementMethod>
{
    public void Configure(EntityTypeBuilder<ProcurementMethod> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Alt).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description_En).HasMaxLength(1000);
        builder.Property(e => e.Description_Alt).HasMaxLength(1000);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
