using JwtTokens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtTokens.Controllers
{
    public interface IToken
    {
        string BuildToken(string key, string issuer, Account user);
        bool IsTokenValid(string key, string issuer, string token);
    }
}
