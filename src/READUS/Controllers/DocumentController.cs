using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Storage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace READUS.Controllers
{
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private IDataContext dataContext;

        public DocumentController(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid id)
        {
            var doc = this.dataContext.Documents.Get(id);

            if (doc == null)
                return NotFound();

            return Ok(doc);
        }

        // POST api/<controller>
        [HttpPost]
        public IActionResult Post([FromBody]Document document)
        {
            if (document == null)
                return BadRequest();

            this.dataContext.Documents.Add(document);
            return Ok(document.Id);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public IActionResult Put([FromBody]Document updatedDocument)
        {
            if (updatedDocument == null)
                return BadRequest();

            this.dataContext.Documents.Update(updatedDocument);
            return Ok();

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var doc = this.dataContext.Documents.Get(id);
            if (doc == null)
                return NotFound();

            this.dataContext.Documents.Delete(doc);
            return Ok();
        }
    }
}
