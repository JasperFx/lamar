using Lamar.IoC;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC;

public class SanitizeMethodTests
{
    [Theory]
    [InlineData("aa]aa", "aa_aa")]
    [InlineData("aa[aa", "aa_aa")]
    [InlineData("aa<aa", "aa_aa")]
    [InlineData("aa>aa", "aa_aa")]
    [InlineData("aa+aa", "aa_aa")]
    [InlineData("aa,aa", "aa_aa")]
    [InlineData("aa`aa", "aa_aa")]
    [InlineData("aa,aa", "aa_aa")]
    public void sanitize(string initial, string expected)
    {
        initial.Sanitize().ShouldBe(expected);
    }
}