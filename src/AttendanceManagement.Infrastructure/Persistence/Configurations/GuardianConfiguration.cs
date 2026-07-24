using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("guardian");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Guardian.NameMaxLength);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Mobile).HasMaxLength(30);
        builder.Property(x => x.Whatsapp).HasMaxLength(30);
        builder.Property(x => x.ContactEmail).HasMaxLength(254);

        builder.OwnsOne(x => x.Address, AddressMapping.Configure);
        builder.Navigation(x => x.Address).IsRequired();

        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_guardian_transporter");
        builder.HasIndex(x => x.UserAccountId).HasDatabaseName("ix_guardian_user_account");

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserAccount)
            .WithMany()
            .HasForeignKey(x => x.UserAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
