using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainObjects
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}
