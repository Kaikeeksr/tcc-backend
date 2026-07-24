using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("school");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(School.NameMaxLength);

        builder.OwnsOne(x => x.Address, AddressMapping.Configure);
        builder.Navigation(x => x.Address).IsRequired();

        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_school_transporter");
        builder.HasIndex(x => new { x.TransporterId, x.Name })
            .IsUnique()
            .HasDatabaseName("ix_school_transporter_name");

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
