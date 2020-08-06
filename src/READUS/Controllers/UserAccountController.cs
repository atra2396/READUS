using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using READUS.Commands;
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
        [HttpPost]
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
    }
}
