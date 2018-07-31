using System.Linq;

namespace SongStatus
{
    public static class StringExtensions
    {
        public static string ReplaceKeyword(this string text, string keyword, string replaceKeyword, string[] keywords)
        {
            if (!keywords.Any(x => x.Contains(keyword))) return text;
            var containingKeywords = keywords.Where(x => x.Contains(keyword));

            if (string.IsNullOrEmpty(replaceKeyword))
            {
                // If the replacement word is null or empty, we want to remove the whole bracket.
                foreach (var containingKeyword in containingKeywords)
                    text = text.Replace("{" + containingKeyword + "}", string.Empty);

                return text;
            }

            foreach (var containingKeyword in containingKeywords)
                text = text.Replace("{" + containingKeyword + "}", containingKeyword);

            text = text.Replace(keyword, replaceKeyword);
            return text;
        }
    }
}
