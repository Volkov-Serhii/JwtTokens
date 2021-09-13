using JwtTokens.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JwtTokens.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IToken _token;
        private AccountContext db;
        private string generatedToken = null;

        public HomeController(IConfiguration config, IToken token, AccountContext context)
        {
            _token = token;
            _config = config;
            db = context;
        }
       

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public IActionResult Login(Login user)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return (RedirectToAction("Error"));
            }
            IActionResult response = Unauthorized();
            var validUser = GetUser(user);
            if (validUser != null)
            {
                generatedToken = _token.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(), validUser);
                if (generatedToken != null)
                {
                    HttpContext.Session.SetString("Token", generatedToken);
                    return RedirectToAction("Main");
                }
                else
                {
                    return (RedirectToAction("Error"));
                }
            }
            else
            {
                return (RedirectToAction("Error"));
            }
        }

        private Account GetUser(Login user)
        {
            return db.Accounts.ToList().SingleOrDefault(u => u.Email == user.Email && u.Password == user.Password);
        }

        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public IActionResult Register(Register user)
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return (RedirectToAction("Error"));
            }
            IActionResult response = Unauthorized();
            Account Account = new Account { Name = user.Name, Email = user.Email, Password = user.Password };
            db.Accounts.Add(Account);
            db.SaveChangesAsync();
            generatedToken = _token.BuildToken(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(), Account);
            if (generatedToken != null)
            {
                HttpContext.Session.SetString("Token", generatedToken);
                return RedirectToAction("Main");
            }
            else
            {
                return (RedirectToAction("Error"));
            }
        }

        [Authorize]
        [Route("main")]
        [HttpGet]
        public IActionResult Main()
        {
            string token = HttpContext.Session.GetString("Token");
            if (token == null)
            {
                return (RedirectToAction("Index"));
            }
            if (!_token.IsTokenValid(_config["Jwt:Key"].ToString(), _config["Jwt:Issuer"].ToString(), token))
            {
                return (RedirectToAction("Index"));
            }
            ViewBag.Message = BuildMessage(token, 50);
            IEnumerable<Account> accounts = db.Accounts;
            ViewBag.Accounts = accounts;
            return View();
        }

        public IActionResult Error()
        {
            ViewBag.Message = "An error occured...";
            return View();
        }

        private string BuildMessage(string stringToSplit, int chunkSize)
        {
            var data = Enumerable.Range(0, stringToSplit.Length / chunkSize).Select(i => stringToSplit.Substring(i * chunkSize, chunkSize));
            string result = "The generated token is:";
            foreach (string str in data)
            {
                result += Environment.NewLine + str;
            }
            return result;
        }
    }
}
