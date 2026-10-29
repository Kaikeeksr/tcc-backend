using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Domain.Enums;
using Xunit;

namespace AttendanceManagement.Domain.Tests.Entities;

public class GuardianStudentTests
{
    [Fact]
    public void Create_with_valid_ids_succeeds_and_starts_active()
    {
        var result = GuardianStudent.Create(Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Mother, isPrimary: true, canPickup: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Active);
        Assert.True(result.Value.CanPickup);
        Assert.True(result.Value.IsPrimary);
        Assert.Null(result.Value.EndedAtUtc);
    }

    [Fact]
    public void Create_requires_guardian()
    {
        var result = GuardianStudent.Create(Guid.Empty, Guid.NewGuid(), RelationshipType.Mother, false, false);

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianStudent.GuardianRequired", result.Error.Code);
    }

    [Fact]
    public void Create_requires_student()
    {
        var result = GuardianStudent.Create(Guid.NewGuid(), Guid.Empty, RelationshipType.Mother, false, false);

        Assert.True(result.IsFailure);
        Assert.Equal("GuardianStudent.StudentRequired", result.Error.Code);
    }

    [Fact]
    public void End_deactivates_and_clears_primary()
    {
        var link = GuardianStudent.Create(Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, isPrimary: true, canPickup: true).Value;

        link.End();

        Assert.False(link.Active);
        Assert.False(link.IsPrimary);
        Assert.NotNull(link.EndedAtUtc);
    }

    [Fact]
    public void End_is_idempotent()
    {
        var link = GuardianStudent.Create(Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, true, true).Value;
        link.End();
        var endedAt = link.EndedAtUtc;

        link.End();

        Assert.Equal(endedAt, link.EndedAtUtc);
    }

    [Fact]
    public void SetCanPickup_updates_flag()
    {
        var link = GuardianStudent.Create(Guid.NewGuid(), Guid.NewGuid(), RelationshipType.Father, false, canPickup: false).Value;

        link.SetCanPickup(true);

        Assert.True(link.CanPickup);
    }
}
