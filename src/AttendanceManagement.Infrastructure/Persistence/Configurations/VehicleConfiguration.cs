using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("vehicle");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Plate).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Model).HasMaxLength(80);

        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_vehicle_transporter");
        builder.HasIndex(x => new { x.TransporterId, x.Plate })
            .IsUnique()
            .HasDatabaseName("ix_vehicle_transporter_plate");

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
