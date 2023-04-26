using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;

namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System;
    using BookShop.Models;
    using System.Globalization;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            //int lengthCheck = int.Parse(Console.ReadLine());
            //Console.WriteLine(GetBooksByAgeRestriction(db, input));
            //Console.WriteLine(GetGoldenBooks(db));
            //Console.WriteLine(GetBooksByPrice(db));
            //Console.WriteLine(GetBooksNotReleasedIn(db, year));
            //Console.WriteLine(GetBooksByCategory(db, input));
            //Console.WriteLine(GetBooksReleasedBefore(db, date));
            //Console.WriteLine(GetAuthorNamesEndingIn(db, input));
            //Console.WriteLine(GetBookTitlesContaining(db, input));
            //Console.WriteLine(GetBooksByAuthor(db, input));
            //Console.WriteLine(CountBooks(db, lengthCheck));
            //Console.WriteLine(CountCopiesByAuthor(db));
            //Console.WriteLine(GetTotalProfitByCategory(db));
            //Console.WriteLine(GetMostRecentBooks(db));
            Console.WriteLine(RemoveBooks(db));
        }
        //2. Age Restriction
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            AgeRestriction ageRestriction;

            bool isEnum = Enum.TryParse<AgeRestriction>(command, true, out ageRestriction);

            if (!isEnum)
                return null;

            List<string> books = context.Books.Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToList();

            StringBuilder output = new StringBuilder();

            foreach (var book in books)
                output.AppendLine(book);

            return output.ToString().TrimEnd();
        }

        //3. Golden Books
        public static string GetGoldenBooks(BookShopContext context)
        {
            List<string> books = context.Books.Where(c => c.Copies < 5000 && c.EditionType == EditionType.Gold)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var bookTitle in books)
                stringBuilder.AppendLine(bookTitle);

            return stringBuilder.ToString().TrimEnd();
        }

        //4. Books by Price
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books.Where(x => x.Price > 40)
                .OrderByDescending(p => p.Price)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                }).ToList();

            StringBuilder stringBuilder = new StringBuilder();

            foreach (var book in books)
                stringBuilder.AppendLine($"{book.Title} - ${book.Price:f2}");

            return stringBuilder.ToString().TrimEnd();
        }

        //5. Not Released In
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books.Where(x => x.ReleaseDate.Value.Year != year)
                .OrderBy(i => i.BookId)
                .Select(b => b.Title)
                .ToList();

           return String.Join(Environment.NewLine, books);
        }

        //6. Book Titles by Category

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categories = input.Split(' ',StringSplitOptions.RemoveEmptyEntries);

            List<string> books = new List<string>();

            foreach (var category in categories)
            {
               var curentBooks = context.Books.Where(x =>
                    x.BookCategories.Any(b => b.Category.Name.ToLower() == category.ToLower())).Select(b => b.Title).ToList();
               books.AddRange(curentBooks);
            }

            books = books.OrderBy(b => b).ToList();

            return String.Join(Environment.NewLine, books);
        }

        //7. Released Before Date
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime time = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var booksList = context.Books.Where(b => DateTime.Compare(b.ReleaseDate.Value, time) < 0)
                .Select(b => new
                {
                    BookTitle = b.Title,
                    BookEdition = b.EditionType,
                    BookPrice = b.Price,
                    ReleaseDate = b.ReleaseDate,
                })
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();

            StringBuilder output = new StringBuilder();

            foreach (var book in booksList)
                output.AppendLine($"{book.BookTitle} - {book.BookEdition} - ${book.BookPrice:f2}");

            return output.ToString().TrimEnd();
            
        }

        //08. Author Search
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var athors = context.Authors.Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName,
                })
                .OrderBy(a => a.FullName)
                .ToList();

            StringBuilder output = new StringBuilder();

            foreach (var athor in athors)
                output.AppendLine($"{athor.FullName}");

            return output.ToString().TrimEnd();
        }

        //9. Book Search
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            List<string> books = context.Books
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (string title in books.Where(t => t.Contains(input, StringComparison.OrdinalIgnoreCase)))
                sb.AppendLine(title);

            return sb.ToString().Trim();
        }

        //10. Book Search by Author
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            input = input.ToLower();

            var books = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input))
                .Select(b => new
                {
                    BookTitle = b.Title,
                    AuthorFullName = $"{b.Author.FirstName} {b.Author.LastName}",
                    BookId = b.BookId
                }).OrderBy(b => b.BookId).ToList();

            StringBuilder output = new StringBuilder();

            foreach (var book in books)
                output.AppendLine($"{book.BookTitle} ({book.AuthorFullName})");

            return output.ToString().Trim();
        }

        //11. Count Books
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books.Where(x => x.Title.Length > lengthCheck).ToList();

            return books.Count;
        }

        //12. Total Book Copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var books = context.Authors
                .Select(a=> new
            {
                AuthorFullName = $"{a.FirstName} {a.LastName}",
                BookCopies = a.Books.Sum(b =>b.Copies)
            })
                .OrderByDescending(b=>b.BookCopies).ToList();

            StringBuilder output = new StringBuilder();

            foreach (var book in books)
                output.AppendLine($"{book.AuthorFullName} - {book.BookCopies}");

            return output.ToString().Trim();
        }

        //13. Profit by Category
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var books = context.Categories
                .Select(x => new
                {
                    CategoryName = x.Name,
                    ProfitBooks = x.CategoryBooks.Sum(b=>b.Book.Price * b.Book.Copies)
                })
                .OrderByDescending(b=>b.ProfitBooks)
                .ThenBy(c=>c.CategoryName).ToList();

            StringBuilder output = new StringBuilder();

            foreach (var book in books)
                output.AppendLine($"{book.CategoryName} ${book.ProfitBooks:F2}");

            return output.ToString().Trim();
        }

        //14. Most Recent Books
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    CategoryName = c.Name,
                    RecentBooks = c.CategoryBooks.OrderByDescending(c => c.Book.ReleaseDate).Take(3)
                        .Select(b => new
                        {
                            BookTitle = b.Book.Title,
                            BookReleaseDate = b.Book.ReleaseDate,
                        })
                });

            StringBuilder output = new StringBuilder();
            foreach (var category in categories)
            {
                output.AppendLine($"--{category.CategoryName}");
                foreach (var book in category.RecentBooks)
                {
                    output.AppendLine($"{book.BookTitle} ({book.BookReleaseDate.Value.Year})");
                }
            }

            return output.ToString().Trim();
        }

        //15. Increase Prices
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books.Where(b=>b.ReleaseDate.Value.Year < 2010).ToList();

            foreach (var book in books)
            {
                book.Price += 5;
                context.SaveChanges();
            }
        }

        //16. Remove Books
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books.Where(b => b.Copies < 4200).ToList();
            context.Books.RemoveRange(books);
            context.SaveChanges();
            return books.Count;
        }
    }
}


