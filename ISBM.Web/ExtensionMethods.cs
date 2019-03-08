using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ISBM.Web
{
    public static class ExtensionMethods
    {
        public static XmlElement ToXmlElement(this string s)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement(string.Empty, "root", string.Empty);
            var text = doc.CreateTextNode(s);
            root.AppendChild(text);
            doc.AppendChild(root);
            return doc.DocumentElement;
        }

        public static XmlElement[] ToXmlElements(this IEnumerable<string> s)
        {
            return s.Select(m => m.ToXmlElement()).ToArray();
        }
    }
}
