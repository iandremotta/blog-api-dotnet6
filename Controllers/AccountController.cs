using System.Text.RegularExpressions;
using Blog.Data;
using Blog.Extension;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;


// {
//     "data": {
//         "user": "laisff@gmail.com",
//         "password": "G}9FW%$ˆZ9A6M@%7%XODO!ˆER"
//     },
//     "errors": []
// }
namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : Controller
    {
        [HttpPost("v1/account/signup")]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model, [FromServices] EmailService emailService, [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-"),
            };

            var password = PasswordGenerator.Generate(25, true, true);
            user.PasswordHash = PasswordHasher.Hash(password);
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                emailService.Send(user.Name, user.Email, "Bem-vindo", password);
                return Ok(new ResultViewModel<dynamic>(new { user = user.Email, password }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("0x599 - E-mail inválido"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("0x600 - Falha interna do servidor"));
            }
        }

        [HttpPost("v1/account/login")]
        public async Task<IActionResult> Login([FromServices] TokenService tokenService, [FromBody] LoginViewModel model, [FromServices] BlogDataContext context)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = await context.Users.AsNoTracking().Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Dados invalidos"));

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            }
            catch
            {
                return BadRequest(new ResultViewModel<string>("Erro"));
            }
        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model, [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);
            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<User>("Falha"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", null));

        }

        // [Authorize(Roles = "user")]
        // [HttpGet("v1/user")]
        // public IActionResult GetUser() => Ok(User.Identity.Name);

        // [Authorize(Roles = "author")]
        // [HttpGet("v1/author")]
        // public IActionResult GetAuthor() => Ok(User.Identity.Name);

        // [Authorize(Roles = "admin")]
        // [HttpGet("v1/admin")]
        // public IActionResult GetAdmin() => Ok(User.Identity.Name);
    }
}