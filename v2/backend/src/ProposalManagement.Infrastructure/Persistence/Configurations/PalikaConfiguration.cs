using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class PalikaConfiguration : IEntityTypeConfiguration<Palika>
{
    public void Configure(EntityTypeBuilder<Palika> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Name_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Name_Mr).HasMaxLength(300);
        builder.Property(e => e.ShortCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(e => e.ShortCode).IsUnique();
        builder.Property(e => e.Type).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.Address_En).HasMaxLength(500);
        builder.Property(e => e.Address_Mr).HasMaxLength(500);
        builder.Property(e => e.ContactPhone).HasMaxLength(20);
        builder.Property(e => e.Website).HasMaxLength(300);
        builder.Property(e => e.PrimaryLanguage).HasMaxLength(5);
        builder.Property(e => e.AlternateLanguage).HasMaxLength(5);
        builder.Property(e => e.ProposalNumberPrefix).HasMaxLength(20).IsRequired();
        builder.Property(e => e.CurrentFinancialYear).HasMaxLength(20).IsRequired();
        builder.Property(e => e.SmsGatewayProvider).HasMaxLength(100);
        builder.Property(e => e.SmsGatewayApiKey).HasMaxLength(500);
    }
}
