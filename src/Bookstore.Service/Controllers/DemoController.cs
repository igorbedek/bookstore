using Bookstore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System.Data.Entity;
using System.Linq;

[Route("Demo/[action]")]
[AllowAnonymous]
public class DemoController : ControllerBase
{
    private readonly IProcessingEngine _processingEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Common.ExecutionContext _executionContext;

    public DemoController(IRhetosComponent<IProcessingEngine> processingEngine, IRhetosComponent<IUnitOfWork> unitOfWork, IRhetosComponent<Common.ExecutionContext> executionContext)
    {
        this._processingEngine = processingEngine.Value;
        this._unitOfWork = unitOfWork.Value;
        this._executionContext = executionContext.Value;
    }

    [HttpGet]
    public string ReadBooks()
    {
        var readCommandInfo = new ReadCommandInfo { DataSource = "Bookstore.Book", ReadTotalCount = true };
        var result = _processingEngine.Execute(readCommandInfo);
        return $"{result.TotalCount} books.";
    }

    [HttpGet]
    public string WriteBook()
    {
        var newBook = new Bookstore.Book { Title = "NewBook" };
        var saveCommandInfo = new SaveEntityCommandInfo { Entity = "Bookstore.Book", DataToInsert = new[] { newBook } };
        _processingEngine.Execute(saveCommandInfo);
        _unitOfWork.CommitAndClose(); // Commits and closes database transaction.
        return "1 book inserted.";
    }

    [HttpGet]
    public Bookstore.BookInfo GetBooks()
    {
        var repo = _executionContext.Repository.Bookstore.BookInfo.Query().OrderByDescending(x => x.NumberOfComments).FirstOrDefault();
        if (repo != null)
        {
            return new()
            {
                ID = repo.ID,
                NumberOfComments = repo.NumberOfComments,
            };
        }
        return null;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<Bookstore.BookInfo> GetBooks(string id)
    {
        var guid = Guid.Parse(id);
        var repo = await _executionContext.Repository.Bookstore.BookInfo.Query().Where(x => x.Base.ID.Equals(guid)).FirstOrDefaultAsync();
        if (repo != null)
        {
            return new()
            {
                ID = repo.ID,
                NumberOfComments = repo.NumberOfComments,
            };
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomBooks(int page = 1, int pageSize = 20
        )
    {
        var totalBookCount = await _executionContext.EntityFrameworkContext.Bookstore_Book.CountAsync();

        var books = await _executionContext.EntityFrameworkContext
            .Bookstore_Book
            .Include(x => x.Author)
            .Include(x => x.Genre)
            .Select(x => new
            {
                ID = x.ID,
                Title = x.Title,
                Author = x.Author.Name,
                Genre = x.Genre.Name,
                NumberOfPages = x.NumberOfPages,
            })
            .OrderBy(x => x.ID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalResultCount = totalBookCount,
            Results = books
        });
    }

    [HttpGet]
    [Route("expected-ratings-better")]
    public async Task<ActionResult<RecordsAndTotalCountResult<Ratings>>> GetExpectedRatings([FromQuery] RequestBaseModel request, CancellationToken cancellationToken)
    {
        var totalBookCount = await _executionContext.EntityFrameworkContext.Bookstore_Book.CountAsync(cancellationToken);
        var books = _executionContext.EntityFrameworkContext.Bookstore_Book
            .Include(x => x.Extension_ForeignBook)
            .Select(x => new Ratings
            {
                ID = x.ID.ToString(),
                Rating = x.Title.IndexOf("Super") >= 0 ? 100 : x.Title.IndexOf("Great") >= 0 ? 50 : x.Extension_ForeignBook != null ? 1.2m : 0,
                Title = x.Title,
            })
            .OrderByDescending(x => x.Rating)
            .Skip((request.skip.HasValue ? (int)request.skip.Value : (request.page.Value -1) * request.psize.Value))
            .Take(request.psize.Value)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Result = books,
            Value = totalBookCount
        });
    }

    private static decimal GetBookRating(string title, bool isForeign)
    {
        decimal rating = 0;

        if (title.IndexOf("Super") >= 0)
        {
            rating += 100;
        }

        if (title.IndexOf("Great") >= 0)
        {
            rating += 50;
        }

        if (isForeign)
        {
            rating *= 1.2m;
        }

        return rating;
    }
    public class RecordsAndTotalCountResult<T>
    {
        public T[] Records { get; set; }

        public int TotalCount { get; set; }
    }

    public class Ratings
    {
        public string Title { get; set; } = default!;
        public decimal? Rating { get; set; }
        public string ID { get; set; } = default!;
    }

    public record RequestBaseModel
    {
        public string? filter { get; init; } = default;
        public string? fparam { get; init; } = default;
        public string? genericfilter { get; init; } = default;
        public string? filters { get; init; } = default;
        public int? top { get; init; }
        public int? skip { get; init; }
        public int? page { get; init; } = 1;
        public int? psize { get; init; } = 50;
        public string sort { get; init; } = default;
    }
}