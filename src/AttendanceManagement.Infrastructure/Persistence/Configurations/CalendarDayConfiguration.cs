using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class CalendarDayConfiguration : IEntityTypeConfiguration<CalendarDay>
{
    public void Configure(EntityTypeBuilder<CalendarDay> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("calendar_day", t =>
            t.HasCheckConstraint("ck_calendar_day_type", "type IN ('SchoolDay','Holiday')"));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(CalendarDay.DescriptionMaxLength);

        builder.HasIndex(x => new { x.TransporterId, x.Date })
            .IsUnique()
            .HasDatabaseName("ix_calendar_day_transporter_date");

        builder.HasOne(x => x.Transporter)
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
