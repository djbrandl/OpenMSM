using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace ISBM.Prototyping
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var salt = Environment.GetEnvironmentVariable("salt", EnvironmentVariableTarget.Machine);
            if (salt == null)
            {
                var saltBytes = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                salt = Convert.ToBase64String(saltBytes);
                Environment.SetEnvironmentVariable("salt", salt, EnvironmentVariableTarget.Machine);
            }

            Console.Write("Enter a password: ");
            var password = Console.ReadLine();
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(password, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
            Console.WriteLine($"Hashed: {hashed}");
            Console.ReadLine();
        }

        private static async Task SendHTTPPost()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var factory = serviceProvider.GetService<IHttpClientFactory>();
            var client = factory.CreateClient();
            var obj = Newtonsoft.Json.JsonConvert.SerializeObject(new { Foo = "Bar" });
            var response = await client.PostAsync("http://httpbin.org/post", new StringContent(obj, Encoding.UTF8, "application/json"));
            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseText);
        }

        private const string XML_FILE_NAME = "Example.xml";
        private static void DoXPathTest()
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
                switch (expr.ReturnType)
                {
                    case XPathResultType.Number:
                        Console.WriteLine(navigator.Evaluate(expr));
                        break;

                    case XPathResultType.NodeSet:
                        var nodes = navigator.Select(expr);
                        while (nodes.MoveNext())
                        {
                            Console.WriteLine(nodes.Current.OuterXml);
                        }
                        break;

                    case XPathResultType.Boolean:
                        if ((bool)navigator.Evaluate(expr))
                            Console.WriteLine("True!");
                        break;

                    case XPathResultType.String:
                        Console.WriteLine(navigator.Evaluate(expr));
                        break;
                }
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
    }
}
