using System.Security.Claims;
using ItBudgetingV4.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ItBudgetingV4.Web.Controllers;

public class AccountController(IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var users = configuration.GetSection("Users").Get<List<AppUser>>() ?? [];
        var user = users.FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public class AppUser
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
