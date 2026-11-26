using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoviesAdmin.Data;
using MoviesAdmin.Services;
using System.Security.Claims;
using MoviesAdmin;
using MoviesAdmin.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MoviesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesContext")));

builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

// Start: Auth0 setup
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    options.Scope = "openid profile email";              // ask Auth0 for the email too

    options.OpenIdConnectEvents = new OpenIdConnectEvents
    {
        // runs once each time someone logs in
        OnTokenValidated = async context =>
        {
            var principal = context.Principal!;
            var db = context.HttpContext.RequestServices.GetRequiredService<MoviesContext>();

            var id = principal.GetUserId();
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                user = new AppUser { Id = id };
                db.Users.Add(user);
            }

            // update every login, so name/email changes in Auth0 carry over
            user.Name = principal.Identity?.Name ?? principal.FindFirstValue("name") ?? "";
            user.Email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email") ?? "";

            await db.SaveChangesAsync();
        }
    };
});

builder.Services.Configure<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "https://whattowatch.com/roles"
    };
});
// End: Auth0 setup

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // new, must come before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")   // Home is now the landing page
    .WithStaticAssets();

app.Run();

