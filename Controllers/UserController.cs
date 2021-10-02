using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {

        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
        {
            var users = await context
            .Users
            .AsNoTracking()
            .ToListAsync();
            return users;
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        // [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Post(
            [FromServices] DataContext context,
            [FromBody] User model)
        {
            // Verifica se os dados são válidos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                //Esconde a senha
                model.Password = "";

                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possivel criar o usuário" });
            }
        }
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate(
          [FromServices] DataContext context,
          [FromBody] User model)
        {
            var user = await context.Users
            .AsNoTracking()
            .Where(x => x.Username == model.Username && x.Password == model.Password)
            .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválidos" });

            var token = TokenService.GenerateToken(user);

            user.Password = "";
            return new
            {
                user = user,
                token = token
            };
        }
    }
}