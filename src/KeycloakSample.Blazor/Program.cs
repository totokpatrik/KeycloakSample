using KeycloakSample.Blazor;
using KeycloakSample.Blazor.Components;
using KeycloakSample.Blazor.HttpService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:50051") });
builder.Services.AddScoped<IHttpService, HttpService>();

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddOpenIdConnect(oidcOptions =>
    {
        oidcOptions.RequireHttpsMetadata = false;
        oidcOptions.Authority = "http://localhost:18080/realms/my-realm";
        oidcOptions.ClientId = "my-client";
        oidcOptions.ClientSecret = "nIKZcMclxcRiP53BT7jYN7b8YK5WnzXO";
        oidcOptions.ResponseType = "code";
        oidcOptions.SaveTokens = true;
        oidcOptions.Scope.Add("openid");
        oidcOptions.CallbackPath = "/login-callback";
        oidcOptions.SignOutScheme = OpenIdConnectDefaults.DisplayName;
        oidcOptions.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
        oidcOptions.SaveTokens = true;
        oidcOptions.RefreshInterval = new TimeSpan(0, 0, 10);
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
