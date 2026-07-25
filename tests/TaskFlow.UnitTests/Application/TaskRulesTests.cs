using FluentAssertions;
using TaskFlow.Application.Common;
using TaskFlow.Application.Validation;
using Xunit;

namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Demonstrates the TDD approach against the business layer: pure rules are
/// tested in isolation, with no database or web host involved.
/// </summary>
public class TaskRulesTests
{
    private static readonly DateTime Now = new(2026, 07, 24, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_ReturnsSuccess_ForValidInput()
    {
        var result = TaskRules.Validate("Write report", "Quarterly numbers", Now.AddDays(1), Now);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Fails_WhenTitleMissing(string title)
    {
        var result = TaskRules.Validate(title, null, null, Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Validate_Fails_WhenTitleTooLong()
    {
        var title = new string('a', TaskRules.TitleMaxLength + 1);

        var result = TaskRules.Validate(title, null, null, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenDueDateInThePast()
    {
        var result = TaskRules.Validate("Valid", null, Now.AddDays(-1), Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("past");
    }

    [Fact]
    public void Validate_AllowsDueDateToday()
    {
        var result = TaskRules.Validate("Valid", null, Now, Now);

        result.IsSuccess.Should().BeTrue();
    }
}
