using System.Text.RegularExpressions;

namespace Lamar.IoC
{
    public static class StringExtensions
    {
        public static string Sanitize(this string value)
        {
            return Regex.Replace(value, @"[\#\<\>\,\.\]\[\`\+]", "_").Replace(" ", "");
        }
    }
}