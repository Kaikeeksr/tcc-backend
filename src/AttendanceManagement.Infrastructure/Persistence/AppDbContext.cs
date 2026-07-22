using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<Transporter> Transporters => Set<Transporter>();

    public DbSet<Assistant> Assistants => Set<Assistant>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<School> Schools => Set<School>();

    public DbSet<Guardian> Guardians => Set<Guardian>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<TransportGroup> TransportGroups => Set<TransportGroup>();

    public DbSet<GuardianStudent> GuardianStudents => Set<GuardianStudent>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    public DbSet<EventLog> EventLogs => Set<EventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
