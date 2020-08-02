namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context
                .Authors
                .Select(a => new ExportAuthorDto
                {
                    Name = string.Format(a.FirstName + " " + a.LastName),
                    Books = a.AuthorsBooks
                        .OrderByDescending(b => b.Book.Price)
                        .Select(ab => new ExportAuthorBooksDto
                        {
                            Name = ab.Book.Name,
                            Price = ab.Book.Price.ToString("f2")
                        })
                        .ToArray()
                })
                .ToArray()
                .OrderByDescending(a => a.Books.Length)
                .ThenBy(a => a.Name)
                .ToArray();

            var jsonConvert = JsonConvert.SerializeObject(authors, Formatting.Indented);

            return jsonConvert;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            ExportBooksDto[] books = context
                .Books
                .Where(b => b.PublishedOn < date && b.Genre == Genre.Science)
                .ToArray()
                .OrderByDescending(b => b.Pages)
                .ThenByDescending(b => b.PublishedOn)
                .Take(10)
                .Select(b => new ExportBooksDto
                {
                    Name = b.Name,
                    Date = b.PublishedOn.ToString("d", CultureInfo.InvariantCulture),
                    Pages = b.Pages
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportBooksDto[]), new XmlRootAttribute("Books"));
            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add(string.Empty, string.Empty);

            using (StringWriter sw = new StringWriter(sb))
            {
                xmlSerializer.Serialize(sw, books, xmlSerializerNamespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}