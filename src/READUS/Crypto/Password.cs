using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace READUS.Crypto
{
    public class Password
    {
        string password;

        public Password(string pass)
        {
            this.password = pass;
        }

        public string CreatePasswordHash(string salt)
        {
            var combinedPassword = this.password + salt;
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                var sha256data = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
                return Encoding.UTF8.GetString(sha256data);
            }
        }
    }
}
