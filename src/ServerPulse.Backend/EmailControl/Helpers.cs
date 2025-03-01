using System.Text.RegularExpressions;

namespace EmailControl
{
    internal class Helpers
    {
        internal static string HtmlToPlainText(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1));
        }
    }
}
