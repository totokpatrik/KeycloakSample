using Keycloak.Blazor.Services;
using KeycloakSample.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"];
    options.ProviderOptions.ClientId = builder.Configuration["Keycloak:resource"];
    options.ProviderOptions.MetadataUrl = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"] + "/.well-known/openid-configuration";

    options.UserOptions.RoleClaim = "roles";
});

builder.Services.AddApiAuthorization().AddAccountClaimsPrincipalFactory<CustomUserFactory>();

await builder.Build().RunAsync();
