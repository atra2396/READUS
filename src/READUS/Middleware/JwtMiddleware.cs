using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Storage;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace READUS.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;

        public JwtMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IDataContext dataContext)
        {
            var token = context?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                GetUser(context, dataContext, token);

            await this.next(context);
        }

        private void GetUser(HttpContext context, IDataContext dataContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("this is the key for now");
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                //var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                //context.Items["User"] = dataContext.Users.GetById(userId);
            }
            catch
            {

            }
        }
    }
}
