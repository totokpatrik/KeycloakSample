using KeycloakSample.Blazor;
using KeycloakSample.Blazor.Components;
using KeycloakSample.Blazor.HttpService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:50000") });
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
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();

//OTEL
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(typeof(Program).Assembly!.GetName().Name!))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        tracing.AddOtlpExporter();
    });

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
