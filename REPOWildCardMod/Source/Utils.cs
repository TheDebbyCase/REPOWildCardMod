//namespace REPOWildCardMod.Utils
//{
//    public class WildCardUtils
//    {
//        public static WildCardUtils instance;
//        public bool TextIsSimilar(string first, string second)
//        {
//            string firstReplaced = first.Replace(" ", string.Empty);
//            string secondReplaced = second.Replace(" ", string.Empty);
//            if (first == second)
//            {
//                return true;
//            }
//            else if (first == second.ToLower() || first == second.ToUpper())
//            {
//                return true;
//            }
//            else if (first == second.TrimStart() || first == second.TrimEnd())
//            {
//                return true;
//            }
//            else if (firstReplaced == secondReplaced)
//            {
//                return true;
//            }
//            else if (firstReplaced == secondReplaced.ToLower() || firstReplaced == secondReplaced.ToUpper())
//            {
//                return true;
//            }
//            else if (firstReplaced == secondReplaced.TrimStart() || firstReplaced == secondReplaced.TrimEnd())
//            {
//                return true;
//            }
//            return false;
//        }
//    }
//}