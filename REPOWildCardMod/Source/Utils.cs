using System.Text.RegularExpressions;
namespace REPOWildCardMod.Utils
{
    public class WildCardUtils
    {
        public static WildCardUtils instance;
        public bool TextIsSimilar(string first, string second)
        {
            string firstReplaced = first.Replace(" ", string.Empty);
            string secondReplaced = second.Replace(" ", string.Empty);
            if (first == second)
            {
                return true;
            }
            else if (first == second.ToLower() || first == second.ToUpper())
            {
                return true;
            }
            else if (first == second.TrimStart() || first == second.TrimEnd())
            {
                return true;
            }
            else if (first == second[1..] || first == second[..(second.Length - 2)])
            {
                return true;
            }
            else if (firstReplaced == secondReplaced)
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.ToLower() || firstReplaced == secondReplaced.ToUpper())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.TrimStart() || firstReplaced == secondReplaced.TrimEnd())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced[1..] || firstReplaced == secondReplaced[..(secondReplaced.Length - 2)])
            {
                return true;
            }
            return false;
        }
        public string CleanText(string text)
        {
            string newText = Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
            if (newText == string.Empty)
            {
                newText = $"this is nicer {text.Length}";
            }
            return newText;
        }
    }
}