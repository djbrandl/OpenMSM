using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace ISBM.Prototyping
{
    class Program
    {
        private const string XML_FILE_NAME = "Example.xml";
        static void Main(string[] args)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, XML_FILE_NAME);
            var xPathDoc = new XPathDocument(filePath);
            Console.WriteLine("Enter an XPath expression");

            var xPath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(xPath))
            {
                xPath = "/bookstore/book/author[../price>10.00]/award/text()";
            }

            try
            {
                var expr = XPathExpression.Compile(xPath);

                var navigator = xPathDoc.CreateNavigator();
                var namespaceManager = new XmlNamespaceManager(navigator.NameTable);
                namespaceManager.AddNamespace("bk", "http://www.contoso.com/books");

                Evaluate(expr, navigator);
                //var iterator = navigator.Select(expr);
                //while (iterator.MoveNext())
                //{
                //    Console.WriteLine(iterator.Current.OuterXml);
                //}
                Console.ReadLine();
            }
            catch (XPathException)
            {
                Console.WriteLine("Invalid XPath. Press ENTER to quit.");
                Console.ReadLine();
                return;
            }

        }

        public static void Evaluate(XPathExpression expression, XPathNavigator navigator)
        {
            switch (expression.ReturnType)
            {
                case XPathResultType.Number:
                    Console.WriteLine(navigator.Evaluate(expression));
                    break;

                case XPathResultType.NodeSet:
                    var nodes = navigator.Select(expression);
                    while (nodes.MoveNext())
                    {
                        Console.WriteLine(nodes.Current.OuterXml);
                    }
                    break;

                case XPathResultType.Boolean:
                    if ((bool)navigator.Evaluate(expression))
                        Console.WriteLine("True!");
                    break;

                case XPathResultType.String:
                    Console.WriteLine(navigator.Evaluate(expression));
                    break;
            }
        }
    }
}
