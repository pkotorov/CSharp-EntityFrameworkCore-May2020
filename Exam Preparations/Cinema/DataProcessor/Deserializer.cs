namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.Data.Models;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Microsoft.EntityFrameworkCore.Internal;
    using Newtonsoft.Json;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie 
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat 
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection 
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket 
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var moviesDto = JsonConvert.DeserializeObject<ImportMovies[]>(jsonString);

            List<Movie> movies = new List<Movie>();

            StringBuilder sb = new StringBuilder();

            foreach (var movieDto in moviesDto)
            {
                if (!IsValid(movieDto)) 
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (movies.Any(t => t.Title == movieDto.Title))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Movie movie = new Movie
                {
                    Title = movieDto.Title,
                    Genre = movieDto.Genre,
                    Duration = movieDto.Duration,
                    Director = movieDto.Director,
                    Rating = movieDto.Rating
                };

                movies.Add(movie);
                sb.AppendLine(string.Format(SuccessfulImportMovie, movieDto.Title, movieDto.Genre.ToString(), movieDto.Rating.ToString("f2")));
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallsDto = JsonConvert.DeserializeObject<ImportHallSeats[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Hall> halls = new List<Hall>();

            foreach (var hallDto in hallsDto)
            {
                if (!IsValid(hallDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Hall hall = new Hall
                {
                    Name = hallDto.Name,
                    Is4Dx = hallDto.Is4Dx,
                    Is3D = hallDto.Is3D,
                };

                for (int i = 0; i < hallDto.Seats; i++)
                {
                    hall.Seats.Add(new Seat());
                }

                halls.Add(hall);

                var type = string.Empty;

                if (hall.Is4Dx && hall.Is3D)
                {
                    type = "4Dx/3D";
                }
                else if (hall.Is4Dx && !hall.Is3D)
                {
                    type = "4Dx";
                }
                else if (!hall.Is4Dx && hall.Is3D)
                {
                    type = "3D";
                }
                else if (!hall.Is4Dx && !hall.Is3D)
                {
                    type = "Normal";
                }

                sb.AppendLine(string.Format(SuccessfulImportHallSeat, hallDto.Name, type, hallDto.Seats));
            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportProjectionsDto[]), new XmlRootAttribute("Projections"));

            var projectionsDto = (ImportProjectionsDto[])serializer.Deserialize(new StringReader(xmlString));

            StringBuilder sb = new StringBuilder();

            List<Projection> projections = new List<Projection>();

            foreach (var projectionDto in projectionsDto)
            {
                var movie = context.Movies.FirstOrDefault(x => x.Id == projectionDto.MovieId);
                var hall = context.Halls.FirstOrDefault(x => x.Id == projectionDto.HallId);

                if (movie == null || hall == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Projection projection = new Projection
                {
                    MovieId = projectionDto.MovieId,
                    HallId = projectionDto.HallId,
                    DateTime = DateTime.ParseExact(projectionDto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };

                projections.Add(projection);

                sb.AppendLine(string.Format(SuccessfulImportProjection, movie.Title, projection.DateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)));
            }

            context.Projections.AddRange(projections);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCustomerTicketsDto[]), new XmlRootAttribute("Customers"));

            ImportCustomerTicketsDto[] customersDtos = null;

            using (StringReader sr = new StringReader(xmlString))
            {
                customersDtos = (ImportCustomerTicketsDto[])serializer.Deserialize(sr);
            }

            StringBuilder sb = new StringBuilder();

            List<Customer> customers = new List<Customer>();

            foreach (var customerDto in customersDtos)
            {
                if (!IsValid(customerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Customer customer = new Customer
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Age = customerDto.Age,
                    Balance = customerDto.Balance
                };

                List<Ticket> tickets = new List<Ticket>();

                foreach (var ticketDto in customerDto.Tickets)
                {
                    if (!IsValid(ticketDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Ticket ticket = new Ticket
                    {
                        ProjectionId = ticketDto.ProjectionId,
                        Price = ticketDto.Price
                    };

                    tickets.Add(ticket);
                }

                customer.Tickets = tickets;

                customers.Add(customer);

                sb.AppendLine(string.Format(SuccessfulImportCustomerTicket, customer.FirstName, customer.LastName, customer.Tickets.Count));
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}