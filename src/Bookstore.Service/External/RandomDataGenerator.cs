using Bogus;
using Common;
using Rhetos.Dom.DefaultConcepts;

namespace Bookstore.Service.External
{
    public static class RandomDataGenerator
    {
        public static void InsertBooks(DomRepository repository, int? numberOfBooks = 100)
        {
            Randomizer.Seed = new Random((int)DateTime.Now.Ticks);
            var authors = repository.Bookstore.Person.Load();
            var genres = repository.Bookstore.Genre.Load();
            var booksFaker = new Faker<Book>()
                .RuleFor(
                    property: u => u.Title,
                    setter: (f, u) => f.WaffleTitle()
                )
                .RuleFor(u => u.ID, (f, u) => f.Random.Guid())
                .RuleFor(u => u.AuthorID, (f, u) => authors[new Random().Next(0, authors.Length)].ID)
                .RuleFor(u => u.NumberOfPages, (f, u) => new Random().Next(0, authors.Length))
                .RuleFor(u => u.GenreID, (f, u) => genres[new Random().Next(0, genres.Length)].ID);

            var books = booksFaker.Generate(numberOfBooks.Value);
            repository.Bookstore.Book.Insert(books);
        }

        public static void InsertAuthors(DomRepository repository, int? numberOfAuthors = 10)
        {
            Randomizer.Seed = new Random((int)DateTime.Now.Ticks);

            var authorsFaker = new Faker<Person>()
                .RuleFor(p => p.Name, (f, p) => f.Person.FullName)
                .RuleFor(p => p.ID, (f, p) => f.Random.Guid());

            var authors = authorsFaker.Generate(numberOfAuthors.Value);
            repository.Bookstore.Person.Insert(authors);
        }
    }
}
