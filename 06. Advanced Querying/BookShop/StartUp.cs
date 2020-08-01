namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            Console.WriteLine(RemoveBooks(db));
        }

        // 02. Age Restriction
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            AgeRestriction are = Enum.Parse<AgeRestriction>(command, true);

            var books = context
                             .Books
                             .Where(ar => ar.AgeRestriction == are)
                             .Select(b => new { b.Title })
                             .OrderBy(b => b.Title)
                             .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        // 03. Golden Books
        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context
                            .Books
                            .Where(e => e.EditionType == EditionType.Gold && e.Copies < 5000)
                            .OrderBy(i => i.BookId)
                            .Select(t => new { t.Title })
                            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        // 04. Books by Price
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context
                            .Books
                            .Where(b => b.Price > 40)
                            .OrderByDescending(p => p.Price)
                            .Select(b => new { b.Title, b.Price })
                            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 05. Not Released In
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context
                            .Books
                            .Where(b => b.ReleaseDate.Value.Year != year)
                            .OrderBy(b => b.BookId)
                            .Select(b => new { b.Title })
                            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().TrimEnd();
        }

        // 06. Book Title by Category
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input
                                .Split(' ')
                                .Select(c => c.ToLower())
                                .ToList();

            var bookTitles = new List<string>();

            foreach (var cat in categories)
            {
                var currentCatTitles = context
                                                    .Books
                                                    .Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == cat))
                                                    .Select(b => b.Title)
                                                    .ToList();

                bookTitles.AddRange(currentCatTitles);
            }

            bookTitles = bookTitles
                                .OrderBy(bt => bt)
                                .ToList();

            var sb = new StringBuilder();

            foreach (var book in bookTitles)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().TrimEnd();
        }

        // 07. Released Before Date
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime dt = DateTime.ParseExact(date, "dd-MM-yyyy", null);

            var books = context
                            .Books
                            .Where(b => b.ReleaseDate < dt)
                            .OrderByDescending(b => b.ReleaseDate)
                            .Select(b => new
                            {
                                b.Title,
                                b.EditionType,
                                b.Price
                            })
                            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 08. Author Search
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context
                                .Authors
                                .Where(a => a.FirstName.EndsWith(input))
                                .OrderBy(a => a.FirstName)
                                .Select(a => $"{ a.FirstName} {a.LastName}")
                                .ToList();

            return string.Join(Environment.NewLine, authors);
        }

        // 09. Book Search
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var titles = context
                            .Books
                            .Where(t => t.Title.ToLower().Contains(input.ToLower()))
                            .Select(t => t.Title)
                            .OrderBy(t => t)
                            .ToList();

            return string.Join(Environment.NewLine, titles);
        }

        // 10. Book Search by Author
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context
                            .Books
                            .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                            .OrderBy(b => b.BookId)
                            .Select(b => new
                            {
                                b.Title,
                                FirstName = b.Author.FirstName,
                                LastName = b.Author.LastName
                            })
                            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.FirstName} {book.LastName})");
            }

            return sb.ToString().TrimEnd();
        }

        // 11. Count Books
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var count = context
                            .Books
                            .Where(b => b.Title.Length > lengthCheck)
                            .ToList()
                            .Count();

            return count;
        }

        // 12. Total Book Copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context
                                .Authors
                                .Select(a => new
                                {
                                    a.FirstName,
                                    a.LastName,
                                    Copies = a.Books.Sum(b => b.Copies)
                                })
                                .OrderByDescending(a => a.Copies)
                                .ToList();

            var sb = new StringBuilder();

            foreach (var aut in authors)
            {
                sb.AppendLine($"{aut.FirstName} {aut.LastName} - {aut.Copies}");
            }

            return sb.ToString().TrimEnd();
        }

        // 13. Profit by Category
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categoriesProfits = context
                                        .Categories
                                        .Select(c => new
                                        {
                                            Category = c.Name,
                                            Profit = c.CategoryBooks.Sum(b => b.Book.Copies * b.Book.Price)
                                        })
                                        .OrderByDescending(p => p.Profit)
                                        .ThenBy(c => c.Category)
                                        .ToList();

            var sb = new StringBuilder();

            foreach (var cat in categoriesProfits)
            {
                sb.AppendLine($"{cat.Category} ${cat.Profit:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 14. Most Recent Books
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context
                            .Categories
                            .Select(c => new
                            {
                                CategoryName = c.Name,
                                Books = c.CategoryBooks
                                            .Select(cb => new
                                            {
                                                Title = cb.Book.Title,
                                                ReleaseDate = cb.Book.ReleaseDate
                                            })
                                            .OrderByDescending(c => c.ReleaseDate)
                                            .Take(3)
                            })
                            .OrderBy(c => c.CategoryName)
                            .ToList();

            var sb = new StringBuilder();

            foreach (var cat in categories)
            {
                sb.AppendLine($"--{cat.CategoryName}");

                foreach (var book in cat.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // 15. Increase Prices
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context
                            .Books
                            .Where(b => b.ReleaseDate.Value.Year < 2010)
                            .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        // 16. Remove Books
        public static int RemoveBooks(BookShopContext context)
        {
            var booksToRemove = context
                                    .Books
                                    .Where(b => b.Copies < 4200)
                                    .ToList();

            var count = booksToRemove.Count;

            foreach (var book in booksToRemove)
            {
                context.Remove(book);
            }

            context.SaveChanges();

            return count;
        }
    }
}
