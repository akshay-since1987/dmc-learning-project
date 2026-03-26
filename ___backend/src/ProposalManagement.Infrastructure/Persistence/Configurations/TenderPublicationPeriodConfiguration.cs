using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class TenderPublicationPeriodConfiguration : IEntityTypeConfiguration<TenderPublicationPeriod>
{
    public void Configure(EntityTypeBuilder<TenderPublicationPeriod> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MinAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.MaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Description_En).HasMaxLength(500);
        builder.Property(e => e.Description_Alt).HasMaxLength(500);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
