using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class TransportGroupConfiguration : IEntityTypeConfiguration<TransportGroup>
{
    public void Configure(EntityTypeBuilder<TransportGroup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("transport_group");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(TransportGroup.NameMaxLength);
        builder.Property(x => x.Shift).HasMaxLength(40);

        builder.HasIndex(x => x.TransporterId).HasDatabaseName("ix_transport_group_transporter");

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Assistant)
            .WithMany()
            .HasForeignKey(x => x.AssistantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAtUtc == null);
    }
}
