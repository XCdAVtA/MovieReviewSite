using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoviesAdmin.Controllers;

public class AccountController : Controller
{
    public async Task Login(string returnUrl = "/")
    {
        var props = new LoginAuthenticationPropertiesBuilder().WithRedirectUri(returnUrl).Build();
        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, props);
    }

    [Authorize]
    public async Task Logout()
    {
        var props = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri(Url.Action("Index", "Home")).Build();
        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, props);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [Authorize]
    public IActionResult Profile() => View();
}