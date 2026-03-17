using ImageMinify.Helpers;

namespace ImageMinify.Tests;

public sealed class FileSizeFormatterTests
{
    [Theory]
    [InlineData(500, "500.00 B")]
    [InlineData(1024, "1.00 KB")]
    [InlineData(1024 * 1024, "1.00 MB")]
    public void Format_ReturnsReadableUnits(long input, string expected)
    {
        var formatted = FileSizeFormatter.Format(input);
        Assert.Equal(expected, formatted);
    }
}
