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
    public class OrganizationController : ControllerBase
    {
        private IDataContext dataContext;

        public OrganizationController(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid id)
        {
            var org = this.dataContext.Organizations.GetById(id);
            if (org == null)
                return NotFound();

            return Ok(org);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]Organization Organization)
        {
            if (Organization == null)
                return BadRequest();

            this.dataContext.Organizations.Add(Organization);
            return Ok(Organization.Id);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public IActionResult Put([FromBody]Organization updatedOrganization)
        {
            if (updatedOrganization == null)
                return BadRequest();

            this.dataContext.Organizations.Update(updatedOrganization);
            return Ok();

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var org = this.dataContext.Organizations.GetById(id);
            if (org == null)
                return NotFound();

            this.dataContext.Organizations.Delete(id);
            return Ok();
        }
    }
}
