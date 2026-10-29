using AttendanceManagement.Domain.Abstractions;
using Xunit;

namespace AttendanceManagement.Domain.Tests.Abstractions;

public class ResultTests
{
    [Fact]
    public void Value_on_failure_throws()
    {
        var result = Result.Failure<int>(Error.Validation("x", "y"));

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Implicit_conversion_wraps_value_as_success()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }
}
