using System.Text.RegularExpressions;

namespace BlogApp.Helpers
{
    public class RemoveHtmlTagHelpers
    {
        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input,"<.*?>|&.*?;", string.Empty); 
        }
    }
}
