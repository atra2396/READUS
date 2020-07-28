using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using DomainObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using READUS.Models;
using Storage;

namespace READUS.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private IDataContext dataContext;

        public AuthController(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody]AuthRequest authRequest)
        {
            if (authRequest == null)
                return BadRequest();

            // string salt = "salty salty";
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
