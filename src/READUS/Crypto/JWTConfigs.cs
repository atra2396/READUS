using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace READUS.Crypto
{
    public class JwtConfigs
    {
        public string SymmetricKey { get; set; }
        public int TokenLifetimeDays { get; set; }
    }
}
