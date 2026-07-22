using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagement.Infrastructure.Persistence.Configurations;

internal sealed class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("event_log");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.AggregateType).IsRequired().HasMaxLength(60);
        builder.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");

        builder.HasIndex(x => new { x.TransporterId, x.OccurredAtUtc })
            .HasDatabaseName("ix_event_log_transporter_date");
        // Linha do tempo de um agregado específico.
        builder.HasIndex(x => new { x.AggregateType, x.AggregateId, x.OccurredAtUtc })
            .HasDatabaseName("ix_event_log_aggregate");

        builder.HasOne<Transporter>()
            .WithMany()
            .HasForeignKey(x => x.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // actor_user_id e aggregate_id são referências soltas (aggregate_id é
        // polimórfico); sem FK de propósito.
    }
}
