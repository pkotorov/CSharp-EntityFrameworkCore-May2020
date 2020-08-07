namespace Cinema.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context
                .Movies
                .Where(x => x.Rating >= rating
                    && x.Projections
                        .Any(y => y.Tickets.Count > 0))
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(f => f.Projections
                    .Sum(ti => ti.Tickets
                        .Sum(p => p.Price)))
                .Select(x => new
                {
                    MovieName = x.Title,
                    Rating = x.Rating.ToString("f2"),
                    TotalIncomes = x.Projections
                        .Sum(y => y.Tickets
                            .Sum(z => z.Price))
                        .ToString("f2"),
                    Customers = x.Projections
                        .SelectMany(t => t.Tickets)
                        .Select(c => new
                        {
                            FirstName = c.Customer.FirstName,
                            LastName = c.Customer.LastName,
                            Balance = c.Customer.Balance
                                .ToString("f2")
                        })
                        .OrderByDescending(b => b.Balance)
                        .ThenBy(f => f.FirstName)
                        .ThenBy(l => l.LastName)
                        .ToArray()
                })
                .Take(10)
                .ToArray();

            var json = JsonConvert.SerializeObject(movies, Formatting.Indented);

            return json;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var customers = context
                .Customers
                .Where(c => c.Age >= age)
                .OrderByDescending(x => x.Tickets
                    .Sum(y => y.Price))
                .Take(10)
                .Select(x => new ExportCustomerDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SpentMoney = x.Tickets
                        .Sum(y => y.Price).ToString("f2"),
                    SpentTime = TimeSpan
                        .FromSeconds(x.Tickets
                            .Sum(d => d.Projection
                                .Movie
                                .Duration
                                .TotalSeconds))
                        .ToString(@"hh\:mm\:ss")
                })
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCustomerDto[]), new XmlRootAttribute("Customers"));

            StringBuilder sb = new StringBuilder();

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (StringWriter sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, customers, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}