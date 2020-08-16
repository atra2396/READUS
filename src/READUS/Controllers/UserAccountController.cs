using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DomainObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using READUS.Commands;
using READUS.Crypto;
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
        CryptoConfigs cryptoSettings;
        JwtConfigs jwtSettings;

        public UserAccountController(IDataContext dataContext, IOptions<CryptoConfigs> cryptoSettings, IOptions<JwtConfigs> jwtSettings)
        {
            this.dataContext = dataContext;
            this.cryptoSettings = cryptoSettings?.Value ?? throw new ArgumentNullException(nameof(cryptoSettings), "Crypto settings are required");
            this.jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings), "JWT settings are required");
        }

        // POST api/<controller>
        [AllowAnonymous]
        [HttpPost("account")]
        public IActionResult CreateAccount([FromBody]CreateAccountCommand account)
        {
            var user = this.dataContext.Users.GetWhere(x => x.Username == account.Username.Trim());
            if (user.Any())
                return Conflict($"Username {account.Username} is already in use");

            var newUser = new User { Username = account.Username, Password = account.Password }; // plain-text for now
            this.dataContext.Users.Add(newUser);

            return Ok(newUser.Id);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody]AuthRequest authRequest)
        {
            if (authRequest == null)
                return BadRequest();

            var encryptedPassword = authRequest.Password + this.cryptoSettings.Salt;

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
            var key = Encoding.UTF8.GetBytes(this.jwtSettings.SymmetricKey);
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
