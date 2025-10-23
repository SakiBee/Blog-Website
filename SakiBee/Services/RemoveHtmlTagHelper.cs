using System.Text.RegularExpressions;

namespace SakiBee.Services
{
    public static class RemoveHtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>|&.*?;", string.Empty);
        }
    }
}
