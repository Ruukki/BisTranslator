using BisTranslator.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BisTranslator.Translator
{
    public static class Translations
    {
        public static Dictionary<string, string> Base = new Dictionary<string, string>()
        {
            {"my", "_'s"},
            {"mine", "_'s"},
            {"me", "_"},
            {"I'm", "_ is"},
            {"Im", "_ is"},
            {"I'll", "_ will"},
            {"I'd", "_ would"},
            {"I've", "_ has"},
            {"I am", "_ is"},
            //{"myself", "itself"},
            {"I", "_"},
            {"am", "is"}
        };

        public static Dictionary<string, string> Named = new Dictionary<string, string>();
        public static void SetName(string name)
        {
            Named = new Dictionary<string, string>();
            foreach (var item in Base)
            {
                Named.Add(item.Key, item.Value.Replace("_", name));
            }
        }

        public static string Translate(string text)
        {
            string prefix = string.Empty;
            if (text.StartsWith("/"))
            {
                if (text.StartsWith("/w "))
                {
                    text = text.Replace("/w ", "/tell ");
                }
                prefix = Regex.Match(text, @"(?<=^|\s)/tell\s{1}\S+\s{1}\S+@\S+(?=\s|$)").Value;
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (prefix.Length == text.Length)
                    {
                        return text;
                    }
                    text = text.Substring(prefix.Length + 1);
                }
            }
            foreach (var word in Named)
            {
                var len = text.Length;
                text = Regex.Replace(text, $@"(?i)(?<=^|\s|\W){word.Key}(?=\s|\W|$)", word.Value);
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                return prefix + " " + text;
            }
            return text;
        }
    }
}
