using KeycloakSample.Blazor;
using KeycloakSample.Blazor.Components;
using KeycloakSample.Blazor.HttpService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(configuration["ProxyUri"] ?? "http://localhost:50000") });
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
        oidcOptions.Authority = $"{configuration["Keycloak:auth-server-url"]}/realms/{configuration["Keycloak:realm"]}";
        oidcOptions.ClientId = configuration["Keycloak:client-id"];
        oidcOptions.ClientSecret = configuration["Keycloak:client-secret"];
        oidcOptions.ResponseType = configuration["Keycloak:response-type"] ?? "code";
        oidcOptions.SaveTokens = true;
        oidcOptions.Scope.Add("openid");
        oidcOptions.CallbackPath = "/login-callback";
        oidcOptions.SignOutScheme = OpenIdConnectDefaults.DisplayName;
        oidcOptions.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimsIdentity.DefaultNameClaimType,
            RoleClaimType = ClaimsIdentity.DefaultRoleClaimType
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
