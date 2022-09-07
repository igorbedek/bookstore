using Bookstore.Repositories;
using Common.Queryable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rhetos;
using Rhetos.Processing;
using Rhetos.Processing.DefaultCommands;
using System.Data.Entity;

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
}