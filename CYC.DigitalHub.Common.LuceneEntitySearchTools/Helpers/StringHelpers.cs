using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Helpers
{
    public static class StringHelpers
    {
        // Remove a set of characters from a string
        public static string RemoveCharacters(this string s, IEnumerable<char> chars)
        {
            if (string.IsNullOrEmpty(s)) { return string.Empty; }
            return new string(s.Where(c => !chars.Contains(c)).ToArray());
        }

        // Remove all HTML tags ( apart from <text> nodes ) and returns the inner html
        public static string RemoveUnwantedTags(string data)
        {
            if (string.IsNullOrEmpty(data)) { return string.Empty; }

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);

                }
            }

            return document.DocumentNode.InnerHtml;
        }
    }
}
