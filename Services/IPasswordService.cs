using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IPasswordService
    {
        bool ValidatePassword(string hash, string password, string salt);
        string CreateSalt();
        string HashPassword(string password, string salt);
    }
}
