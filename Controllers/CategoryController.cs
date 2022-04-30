using Blog.Data;
using Blog.Extension;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : Controller
    {
        // [HttpGet("v1/categories")]
        // public async Task<IActionResult> GetAsync([FromServices] BlogDataContext context)
        // {
        //     var categories = await context.Categories.ToListAsync();
        //     return Ok(categories);
        // }

        // [HttpGet("v1/categories/{id:int}")]
        // public async Task<IActionResult> GetByIdAsync([FromRoute] int id, [FromServices] BlogDataContext context)
        // {
        //     var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        //     if (category == null)
        //     {
        //         return NotFound();
        //     }
        //     return Ok(category);
        // }

        // [HttpPost("v1/categories")]
        // public async Task<IActionResult> PostAsync([FromBody] Category model, [FromServices] BlogDataContext context)
        // {
        //     try
        //     {
        //         await context.Categories.AddAsync(model);
        //         await context.SaveChangesAsync();
        //         return Created($"v1/categories/{model.Id}", model);
        //     }
        //     catch (DbUpdateException ex)
        //     {
        //         return StatusCode(500, "Não foi possível completar a ação");
        //     }
        //     catch (Exception e)
        //     {
        //         return StatusCode(500, "Não foi possível completar a ação");
        //     }
        // }

        // [HttpPut("v1/categories/{id:int}")]
        // public async Task<IActionResult> PutAsync([FromRoute] int id, [FromBody] Category model, [FromServices] BlogDataContext context)
        // {
        //     var category = context.Categories.FirstOrDefault(x => x.Id == id);
        //     if (category == null)
        //     {
        //         return NotFound();
        //     }

        //     category.Name = model.Name;
        //     category.Slug = model.Slug;

        //     context.Categories.Update(category);
        //     await context.SaveChangesAsync();
        //     return Created("", model);
        // }

        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAllAsync([FromServices] BlogDataContext context)
        {
            try
            {
                var categories = await context.Categories.ToListAsync();
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna do servidor"));
            }
        }

        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIDAsync([FromRoute] int id, [FromServices] BlogDataContext context)
        {
            try
            {
                var categorie = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
                if (categorie == null) return BadRequest(new ResultViewModel<List<Category>>("Usuário não encontrado"));

                return Ok(categorie);
            }
            catch
            {
                return BadRequest(new ResultViewModel<List<Category>>(ModelState.GetErrors()));
            }


        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync([FromBody] CreateCategoryViewModel model, [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid) return BadRequest(new ResultViewModel<List<Category>>(ModelState.GetErrors()));
            var category = new Category
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug,
            };
            await context.Categories.AddAsync(category);
            context.SaveChanges();
            return StatusCode(200);
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<ActionResult> Update([FromBody] CreateCategoryViewModel model, [FromRoute] int id, [FromServices] BlogDataContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category != null)
            {
                category.Name = model.Name;
                category.Slug = model.Slug;
                context.Categories.Update(category);
                context.SaveChanges();
                return Ok();
            }
            return StatusCode(404);
        }

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromServices] BlogDataContext context)
        {
            var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (model != null)
            {
                context.Categories.Remove(model);
                context.SaveChanges();
                return StatusCode(200);
            }
            return StatusCode(404);
        }

    }
}