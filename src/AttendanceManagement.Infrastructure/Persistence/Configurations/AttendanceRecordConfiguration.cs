using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("attendance_record", t =>
            t.HasCheckConstraint(
                "ck_attendance_record_status",
                "status IN ('Present','Absent','Late','PickedUpByGuardian','Justified')"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Justification).HasMaxLength(AttendanceRecord.JustificationMaxLength);

        builder.HasIndex(x => new { x.AttendanceSessionId, x.StudentId })
            .IsUnique()
            .HasDatabaseName("ix_attendance_record_session_student");
        builder.HasIndex(x => new { x.StudentId, x.RecordedAtUtc })
            .HasDatabaseName("ix_attendance_record_student_date");
        builder.HasIndex(x => new { x.TransporterId, x.RecordedAtUtc })
            .HasDatabaseName("ix_attendance_record_transporter_date");

        // Parcial: a tela "quem foi retirado" varre só as linhas retiradas.
        builder.HasIndex(x => new { x.TransporterId, x.RecordedAtUtc })
            .HasFilter("status = 'PickedUpByGuardian'")
            .HasDatabaseName("ix_attendance_record_picked_up");

        builder.HasOne(x => x.AttendanceSession)
            .WithMany()
            .HasForeignKey(x => x.AttendanceSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PickedUpByGuardian)
            .WithMany()
            .HasForeignKey(x => x.PickedUpByGuardianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.JustifiedByGuardian)
            .WithMany()
            .HasForeignKey(x => x.JustifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecordedByUser)
            .WithMany()
            .HasForeignKey(x => x.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // school_id é snapshot da escola do aluno no dia; sem FK de propósito.
    }
}
