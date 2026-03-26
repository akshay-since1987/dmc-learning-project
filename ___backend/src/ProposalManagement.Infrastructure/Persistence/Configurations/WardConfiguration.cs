using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Alt).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => e.Number).IsUnique();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
