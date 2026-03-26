using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name_Alt).HasMaxLength(200).IsRequired();
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
