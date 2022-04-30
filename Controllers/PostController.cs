using blog.ViewModels.Posts;
using Blog.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.ViewModels;
using Blog.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;
[ApiController]
public class PostController : ControllerBase
{

    private List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }
    [HttpGet("v1/posts")]
    public async Task<IActionResult> Get([FromServices] IMemoryCache cache, [FromServices] BlogDataContext context, [FromQuery] int page = 0, [FromQuery] int pageSize = 25)
    {
        var count = await context.Posts.AsNoTracking().CountAsync();
        var posts = await context.Posts
          .Include(x => x.Category)
          .Include(x => x.Author)
          .Select(x => new ListPostsViewModel
          {
              Id = x.Id,
              Title = x.Title,
              Slug = x.Slug,
              LastUpdateDate = x.LastUpdateDate,
              Category = x.Category.Name,
              Author = $"{x.Author.Name} - ({x.Author.Email})"
          })
          .Skip(page * pageSize)
          .Take(pageSize)
          .OrderByDescending(x => x.LastUpdateDate)
          .ToListAsync();

        return Ok(new ResultViewModel<dynamic>(new
        {
            total = count,
            page,
            pageSize,
            posts
        }));
    }
    [HttpGet("v1/cache/categories")]
    public async Task<IActionResult> GetCacheAsync([FromServices] BlogDataContext context, [FromServices] IMemoryCache cache)
    {
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });
            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna"));
        }
    }

    [HttpGet("v1/posts/{id:int}")]
    public async Task<IActionResult> DetailsAsync([FromServices] BlogDataContext context, [FromRoute] int id)
    {
        try
        {
            var post = await context.Posts
              .Include(x => x.Category)
              .Include(x => x.Author)
              .ThenInclude(x => x.Roles)
              .OrderByDescending(x => x.LastUpdateDate)
              .FirstOrDefaultAsync(x => x.Id == id);
            if (post == null)
                return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));

            return Ok(new ResultViewModel<Post>(post));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
        }
    }

    [HttpGet("v1/posts/categories/{category}")]
    public async Task<IActionResult> GetByCategoryASync([FromServices] BlogDataContext context,
    [FromRoute] string category,
    [FromQuery] int page = 0,
    [FromQuery] int pageSize = 25)
    {
        var count = await context.Posts.AsNoTracking().CountAsync();
        var posts = await context.Posts
          .Include(x => x.Category)
          .Include(x => x.Author)
          .Where(x => x.Category.Slug == category)
          .Select(x => new ListPostsViewModel
          {
              Id = x.Id,
              Title = x.Title,
              Slug = x.Slug,
              LastUpdateDate = x.LastUpdateDate,
              Category = x.Category.Name,
              Author = $"{x.Author.Name} - ({x.Author.Email})"
          })
          .Skip(page * pageSize)
          .Take(pageSize)
          .OrderByDescending(x => x.LastUpdateDate)
          .ToListAsync();

        return Ok(new ResultViewModel<dynamic>(new
        {
            total = count,
            page,
            pageSize,
            posts
        }));
    }
}