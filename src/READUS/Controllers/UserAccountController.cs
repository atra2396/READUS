using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DomainObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using READUS.Commands;
using READUS.Models;
using Storage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace READUS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UserAccountController : ControllerBase
    {
        IDataContext dataContext;

        public UserAccountController(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        // POST api/<controller>
        [HttpPost("account")]
        [AllowAnonymous]
        public IActionResult CreateAccount([FromBody]CreateAccountCommand account)
        {
            var user = this.dataContext.Users.GetWhere(x => x.Username == account.Username.Trim());
            if (user.Any())
                return Conflict($"Username {account.Username} is already in use");

            var newUser = new User { Username = account.Username, Password = account.Password }; // plain-text for now
            this.dataContext.Users.Add(newUser);

            return Ok(newUser.Id);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody]AuthRequest authRequest)
        {
            if (authRequest == null)
                return BadRequest();

            var encryptedPassword = authRequest.Password; // var encryptedPassword = SomeHash(authRequest.Password + salt)

            var user = this.dataContext.Users
                .GetWhere(x => x.Username == authRequest.Username && x.Password == encryptedPassword)
                .FirstOrDefault();

            if (user == null)
                return BadRequest("Username or password is incorrect");

            var token = GenerateJwt(user);

            var cookieOptions = new CookieOptions
            {
                Domain = this.Request.Host.Host,
                Expires = DateTimeOffset.Now.AddMinutes(5),
                HttpOnly = true,
                Secure = false // will be true for production usage
            };

            this.Response.Cookies.Append("Authorization", token, cookieOptions);

            return Ok();
        }

        private string GenerateJwt(User user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("this is the key that I will use for development");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jwtHandler.CreateToken(tokenDescriptor);
            return jwtHandler.WriteToken(token);
        }
    }
}
