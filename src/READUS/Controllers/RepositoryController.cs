using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace READUS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class RepositoryController : ControllerBase
    {
        private IDataContext dataContext;

        public RepositoryController(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid id)
        {
            var repo = this.dataContext.Repositories.GetById(id);
            if (repo == null)
                return NotFound();

            return Ok(repo);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]Repository Repository)
        {
            if (Repository == null)
                return BadRequest();

            this.dataContext.Repositories.Add(Repository);
            return Ok(Repository.Id);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public IActionResult Put([FromBody]Repository updatedRepository)
        {
            if (updatedRepository == null)
                return BadRequest();

            this.dataContext.Repositories.Update(updatedRepository);
            return Ok();

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var repo = this.dataContext.Repositories.GetById(id);
            if (repo == null)
                return NotFound();

            this.dataContext.Repositories.Delete(id);
            return Ok();
        }
    }
}
