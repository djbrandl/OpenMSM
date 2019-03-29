using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace OpenMSM.Web.Services
{
    public static class ExtensionMethods
    {
        public static XmlElement ToXmlElement(this string s)
        {
            var doc = new XmlDocument();
            if (s.IsValidXml())
            {
                doc.LoadXml(s);
            }
            else
            {
                var root = doc.CreateElement(string.Empty, "root", string.Empty);
                var text = doc.CreateTextNode(s);
                root.AppendChild(text);
                doc.AppendChild(root);
            }
            return doc.DocumentElement;
        }

        public static XmlElement[] ToXmlElements(this IEnumerable<string> s)
        {
            return s.Select(m => m.ToXmlElement()).ToArray();
        }

        public static bool IsGuid(this string s)
        {
            return Guid.TryParse(s, out Guid g);
        }

        public static bool IsValidXml(this string s)
        {
            try
            {
                XDocument.Parse(s);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
