using PaquitoBot.Application.Common.Text;

namespace PaquitoBot.Application.Tests.Common;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Active", "active")]
    [InlineData("ACTIVE", "active")]
    [InlineData("  Enrolled ", "enrolled")]
    public void OptionalLower_TrimsAndLowercases(string input, string expected)
        => Assert.Equal(expected, TextNormalizer.OptionalLower(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OptionalLower_BlankBecomesNull(string? input)
        => Assert.Null(TextNormalizer.OptionalLower(input));
}
