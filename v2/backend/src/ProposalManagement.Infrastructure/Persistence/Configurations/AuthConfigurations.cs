using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProposalManagement.Domain.Entities;

namespace ProposalManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.FullName_En).HasMaxLength(200).IsRequired();
        builder.Property(e => e.FullName_Mr).HasMaxLength(200);
        builder.Property(e => e.MobileNumber).HasMaxLength(15).IsRequired();
        builder.HasIndex(e => new { e.PalikaId, e.MobileNumber }).IsUnique();
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
        builder.Property(e => e.Role).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => new { e.PalikaId, e.Role });
        builder.Property(e => e.SignaturePath).HasMaxLength(500);

        builder.HasOne(e => e.Palika).WithMany(p => p.Users).HasForeignKey(e => e.PalikaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Designation).WithMany().HasForeignKey(e => e.DesignationId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OtpRequestConfiguration : IEntityTypeConfiguration<OtpRequest>
{
    public void Configure(EntityTypeBuilder<OtpRequest> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.MobileNumber).HasMaxLength(15).IsRequired();
        builder.HasIndex(e => e.MobileNumber);
        builder.Property(e => e.OtpHash).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Purpose).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.ExpiresAt);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => e.Token).IsUnique();
        builder.HasIndex(e => e.ExpiresAt);

        builder.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
