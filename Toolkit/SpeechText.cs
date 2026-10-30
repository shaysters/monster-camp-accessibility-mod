using System.Text;

namespace GameMod.Toolkit
{
    internal static class SpeechText
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return StripMarkup(text).Replace('\r', ' ').Replace('\n', ' ').Trim();
        }

        public static string StripMarkup(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(text.Length);
            var inTag = false;
            foreach (var ch in text)
            {
                if (ch == '[')
                {
                    inTag = true;
                    continue;
                }

                if (inTag)
                {
                    if (ch == ']')
                    {
                        inTag = false;
                    }

                    continue;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string CleanObjectName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var cleaned = name.Replace('_', ' ');
            cleaned = cleaned.Replace("Button", string.Empty);
            cleaned = cleaned.Replace("Label", string.Empty);
            cleaned = cleaned.Replace("Color", string.Empty);
            return Normalize(cleaned);
        }

        public static string TitleCaseId(string id)
        {
            var normalized = Normalize(id).Replace('_', ' ');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            var words = normalized.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(words[i]))
                {
                    continue;
                }

                var lower = words[i].ToLowerInvariant();
                words[i] = char.ToUpperInvariant(lower[0]) + lower.Substring(1);
            }

            return Normalize(string.Join(" ", words));
        }
    }
}
