using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("attendance_session", t =>
        {
            t.HasCheckConstraint("ck_attendance_session_type", "session_type IN ('ToSchool','FromSchool')");
            t.HasCheckConstraint("ck_attendance_session_status", "status IN ('Open','Closed','Canceled')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.SessionType).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.SessionDate).IsRequired();

        // Uma ida e uma volta por grupo por dia.
        builder.HasIndex(x => new { x.TransportGroupId, x.SessionDate, x.SessionType })
            .IsUnique()
            .HasDatabaseName("ix_attendance_session_group_date_type");
        builder.HasIndex(x => new { x.TransporterId, x.SessionDate })
            .HasDatabaseName("ix_attendance_session_transporter_date");

        builder.HasOne<Transporter>()
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TransportGroup>()
            .WithMany()
            .HasForeignKey(x => x.TransportGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // vehicle_id e assistant_id são snapshots; sem FK de propósito, para o
        // histórico sobreviver a mudanças na designação do grupo.
    }
}
