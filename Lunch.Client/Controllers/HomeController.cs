

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Lunch.Client.Models;
using Microsoft.AspNetCore.Authentication;
using Lunch.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Lunch.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userName, string password)
        {
            var user = _userService.Login(userName, _userService.Encrypt(password));
            if (user == null) return Ok(0);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            identity.AddClaim(new Claim("UserId", user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.RoleId.ToString()));
            HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return Ok(1);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return Redirect("/Home/Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string address, string phone)
        {
            var result = _userService.Register(
                username, 
                _userService.Encrypt(password), 
                address,
                phone, 
                2);
            return Ok(result);
        }

        [Authorize]
        public IActionResult ChangePassword(string password, string newPassword)
        {
            var userIdStr = User.Claims.SingleOrDefault(s => s.Type == "UserId").Value;
            int.TryParse(userIdStr, out int userId);

            var result = _userService.ChangePassword(
                userId, 
                _userService.Encrypt(password), 
                _userService.Encrypt(newPassword));
            return Ok(true);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Authorize]
        public IActionResult UpdatePwd()
        {
            return View();
        }
    }
}
