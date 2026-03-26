using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class CorporationSettingsConfiguration : IEntityTypeConfiguration<CorporationSettings>
{
    public void Configure(EntityTypeBuilder<CorporationSettings> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.CorporationName_En).HasMaxLength(300).IsRequired();
        builder.Property(e => e.CorporationName_Alt).HasMaxLength(300).IsRequired();
        builder.Property(e => e.PrimaryLanguage).HasMaxLength(5).HasDefaultValue("en");
        builder.Property(e => e.AlternateLanguage).HasMaxLength(5);
        builder.Property(e => e.AlternateLanguageLabel).HasMaxLength(50);
        builder.Property(e => e.DefaultDisplayLanguage).HasMaxLength(5);
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.SmsGatewayProvider).HasMaxLength(100);
        builder.Property(e => e.SmsGatewayApiKey).HasMaxLength(500);
    }
}
