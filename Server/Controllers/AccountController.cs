using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using IdentityWithCookies.Server.Entities;
using IdentityWithCookies.Shared;

namespace IdentityWithCookies.Server.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("v1/[controller]")]
    public class AccountController : Controller
    {
        private UserManager<AccountEntity> UserManager;
    
        public AccountController(UserManager<AccountEntity> userManager)
        {
            UserManager = userManager;
        }

        [HttpGet("hasAdminAccount")]
        async public Task<IActionResult> HasAdminAccount()
        {
            bool hasUsersInAdminRole = (await UserManager
                .GetUsersInRoleAsync(AuthorizationRoles.Admin.ToString())).Count > 0;

            return Ok(hasUsersInAdminRole);
        }

        [HttpPost("register")]
        async public Task<IActionResult> Register(RegisterAccountFormDto dto)
        {
            if (await UserManager.FindByEmailAsync(dto.Email) is not null) {
                return BadRequest($"{dto.Email} already exists");
            } else {
                AccountEntity account = new() { UserName = dto.Email, Email = dto.Email }; 

                IdentityResult result = await UserManager.CreateAsync(account, dto.Password);

                if (result.Succeeded) {
                    account = await UserManager.FindByEmailAsync(dto.Email);

                    await UserManager.AddToRoleAsync(account, AuthorizationRoles.Admin.ToString());
                    await UserManager.UpdateAsync(account);

                    return Ok();
                } else {
                    Console.WriteLine(result.Errors.First().Description);

                    return BadRequest($"Issue adding {dto.Email}");
                }
            }
        }

        [HttpPost("login")]
        async public Task<IActionResult> Login(LoginAccountFormDto dto)
        {
            AccountEntity account = await UserManager.FindByEmailAsync(dto.Email);

            if (await UserManager.CheckPasswordAsync(account, dto.Password)) {
                string jwt = await JwtToken.GenerateAsync(account);

                Response.Cookies.Append("JwtBearer", jwt, new()
                {
                    Expires = DateTime.Now.AddDays(3),
                    IsEssential = true,
                    Path = "/",
                    HttpOnly = false
                });

                return Ok();
            } else {
                return Unauthorized();
            }
        }

        [HttpGet("logout")]
        async public Task<IActionResult> Logout()
        {
            if (HttpContext.Request.Cookies.ContainsKey("JwtBearer")) {
                Response.Cookies.Delete("JwtBearer");
            }

            return Ok();
        }
    }
}
